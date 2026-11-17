package services

import (
	"sort"
	"time"

	"library-management-api/internal/apperr"
	"library-management-api/internal/models"
	"library-management-api/internal/storage"
)

const (
	MaxActiveLoans  = 5
	DefaultLoanDays = 14
	MaxLoanDays     = 60
	TopBooksCount   = 3
)

var monthNames = [12]string{"Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
	"Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"}

type LoanService struct {
	store *storage.Memory
	now   func() time.Time
}

func NewLoanService(store *storage.Memory) *LoanService {
	return &LoanService{store: store, now: time.Now}
}

func (s *LoanService) GetAll() []models.Loan {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	return filterLoans(s.store.Loans, func(models.Loan) bool { return true })
}

func (s *LoanService) Active() []models.Loan {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	return filterLoans(s.store.Loans, func(l models.Loan) bool { return !l.IsReturned })
}

func (s *LoanService) GetByID(id int) (*models.Loan, error) {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	loan, ok := s.store.Loans[id]
	if !ok {
		return nil, apperr.NotFound("LOAN_NOT_FOUND", "Выдача не найдена")
	}
	return &loan, nil
}

func (s *LoanService) Borrow(in models.LoanInput) (*models.Loan, error) {
	if in.BookID <= 0 || in.ReaderID <= 0 {
		return nil, apperr.BadRequest("VALIDATION_ERROR", "Поля bookId и readerId обязательны")
	}
	if in.Days == 0 {
		in.Days = DefaultLoanDays
	}
	if in.Days < 1 || in.Days > MaxLoanDays {
		return nil, apperr.BadRequest("VALIDATION_ERROR", "days должно быть от 1 до 60")
	}

	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	book, ok := s.store.Books[in.BookID]
	if !ok {
		return nil, bookNotFound()
	}
	reader, ok := s.store.Readers[in.ReaderID]
	if !ok {
		return nil, readerNotFound()
	}
	if !reader.IsActive {
		return nil, apperr.Conflict("READER_INACTIVE", "Читатель неактивен")
	}
	if countActiveLoans(s.store.Loans, reader.ID) >= MaxActiveLoans {
		return nil, apperr.Conflict("LOAN_LIMIT_EXCEEDED", "У читателя уже 5 активных выдач")
	}
	if err := takeCopy(&book); err != nil {
		return nil, err
	}

	now := s.now()
	loan := models.Loan{
		ID:         s.store.NextLoanID(),
		BookID:     book.ID,
		ReaderID:   reader.ID,
		BorrowedAt: now,
		DueDate:    now.AddDate(0, 0, in.Days),
	}
	s.store.Books[book.ID] = book
	s.store.Loans[loan.ID] = loan
	return &loan, nil
}

func (s *LoanService) Return(id int) (*models.Loan, error) {
	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	loan, ok := s.store.Loans[id]
	if !ok {
		return nil, apperr.NotFound("LOAN_NOT_FOUND", "Выдача не найдена")
	}
	if loan.IsReturned {
		return nil, apperr.Conflict("LOAN_ALREADY_RETURNED", "Книга уже возвращена")
	}
	markReturned(&loan, s.now())
	if book, ok := s.store.Books[loan.BookID]; ok {
		returnCopy(&book)
		s.store.Books[book.ID] = book
	}
	s.store.Loans[id] = loan
	return &loan, nil
}

func (s *LoanService) Statistics() models.Statistics {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	now := s.now()
	stats := models.Statistics{
		TotalBooks:   len(s.store.Books),
		TotalReaders: len(s.store.Readers),
	}
	for _, b := range s.store.Books {
		stats.TotalCopies += b.TotalCopies
		stats.AvailableCopies += b.AvailableCopies
	}
	stats.AvailabilityPercent = availabilityPercent(stats.AvailableCopies, stats.TotalCopies)
	for _, l := range s.store.Loans {
		if !l.IsReturned {
			stats.ActiveLoans++
			if isOverdue(l, now) {
				stats.OverdueLoans++
			}
		}
	}
	stats.TopBooks = topBooks(s.store.Loans, s.store.Books, TopBooksCount)
	byMonth := loansByMonth(s.store.Loans, now.Year())
	stats.LoansByMonth = make([]models.MonthStat, 0, len(byMonth))
	for i, count := range byMonth {
		stats.LoansByMonth = append(stats.LoansByMonth, models.MonthStat{Month: monthNames[i], Loans: count})
	}
	return stats
}

func takeCopy(book *models.Book) error {
	if book.AvailableCopies == 0 {
		return apperr.Conflict("BOOK_NOT_AVAILABLE", "Нет доступных экземпляров книги")
	}
	book.AvailableCopies--
	return nil
}

func returnCopy(book *models.Book) {
	if book.AvailableCopies < book.TotalCopies {
		book.AvailableCopies++
	}
}

func markReturned(loan *models.Loan, at time.Time) {
	loan.IsReturned = true
	loan.ReturnedAt = &at
}

func countActiveLoans(loans map[int]models.Loan, readerID int) int {
	count := 0
	for _, l := range loans {
		if l.ReaderID == readerID && !l.IsReturned {
			count++
		}
	}
	return count
}

func isOverdue(loan models.Loan, now time.Time) bool {
	return !loan.IsReturned && now.After(loan.DueDate)
}

func availabilityPercent(available, total int) float64 {
	if total == 0 {
		return 0
	}
	return float64(int(float64(available)/float64(total)*1000+0.5)) / 10
}

func loansByMonth(loans map[int]models.Loan, year int) [12]int {
	var months [12]int
	for _, l := range loans {
		if l.BorrowedAt.Year() == year {
			months[l.BorrowedAt.Month()-1]++
		}
	}
	return months
}

func topBooks(loans map[int]models.Loan, books map[int]models.Book, limit int) []models.TopBook {
	counts := make(map[int]int)
	for _, l := range loans {
		counts[l.BookID]++
	}
	result := make([]models.TopBook, 0, len(counts))
	for bookID, n := range counts {
		title := "(удалена)"
		if b, ok := books[bookID]; ok {
			title = b.Title
		}
		result = append(result, models.TopBook{BookID: bookID, Title: title, Loans: n})
	}
	sort.Slice(result, func(i, j int) bool {
		if result[i].Loans != result[j].Loans {
			return result[i].Loans > result[j].Loans
		}
		return result[i].BookID < result[j].BookID
	})
	if len(result) > limit {
		result = result[:limit]
	}
	return result
}

func filterLoans(loans map[int]models.Loan, keep func(models.Loan) bool) []models.Loan {
	result := make([]models.Loan, 0)
	for _, l := range loans {
		if keep(l) {
			result = append(result, l)
		}
	}
	sort.Slice(result, func(i, j int) bool { return result[i].ID < result[j].ID })
	return result
}
