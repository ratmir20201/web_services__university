package services

import (
	"sort"
	"strings"
	"time"

	"library-management-api/internal/apperr"
	"library-management-api/internal/models"
	"library-management-api/internal/storage"
)

type BookService struct {
	store *storage.Memory
}

func NewBookService(store *storage.Memory) *BookService {
	return &BookService{store: store}
}

func (s *BookService) GetAll() []models.Book {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	return sortedBooks(s.store.Books, models.BookFilter{})
}

func (s *BookService) GetByID(id int) (*models.Book, error) {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	book, ok := s.store.Books[id]
	if !ok {
		return nil, bookNotFound()
	}
	return &book, nil
}

func (s *BookService) Search(filter models.BookFilter) []models.Book {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	return sortedBooks(s.store.Books, filter)
}

func (s *BookService) Create(in models.BookInput) (*models.Book, error) {
	if err := validateBook(in); err != nil {
		return nil, err
	}
	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	book := models.Book{ID: s.store.NextBookID()}
	applyBookInput(&book, in)
	book.AvailableCopies = in.TotalCopies
	s.store.Books[book.ID] = book
	return &book, nil
}

func (s *BookService) Update(id int, in models.BookInput) (*models.Book, error) {
	if err := validateBook(in); err != nil {
		return nil, err
	}
	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	book, ok := s.store.Books[id]
	if !ok {
		return nil, bookNotFound()
	}
	onLoan := book.TotalCopies - book.AvailableCopies
	if in.TotalCopies < onLoan {
		return nil, apperr.Conflict("COPIES_IN_USE", "Нельзя сделать экземпляров меньше, чем сейчас выдано")
	}
	applyBookInput(&book, in)
	book.AvailableCopies = in.TotalCopies - onLoan
	s.store.Books[id] = book
	return &book, nil
}

func (s *BookService) Delete(id int) error {
	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	if _, ok := s.store.Books[id]; !ok {
		return bookNotFound()
	}
	for _, loan := range s.store.Loans {
		if loan.BookID == id && !loan.IsReturned {
			return apperr.Conflict("BOOK_HAS_ACTIVE_LOANS", "У книги есть активные выдачи")
		}
	}
	delete(s.store.Books, id)
	return nil
}

func validateBook(in models.BookInput) error {
	switch {
	case strings.TrimSpace(in.Title) == "":
		return apperr.BadRequest("VALIDATION_ERROR", "Поле title обязательно")
	case strings.TrimSpace(in.Author) == "":
		return apperr.BadRequest("VALIDATION_ERROR", "Поле author обязательно")
	case strings.TrimSpace(in.Genre) == "":
		return apperr.BadRequest("VALIDATION_ERROR", "Поле genre обязательно")
	case in.Year < 1 || in.Year > time.Now().Year():
		return apperr.BadRequest("VALIDATION_ERROR", "Некорректный год издания")
	case in.TotalCopies < 1:
		return apperr.BadRequest("VALIDATION_ERROR", "totalCopies должно быть больше 0")
	}
	return nil
}

func applyBookInput(book *models.Book, in models.BookInput) {
	book.Title = strings.TrimSpace(in.Title)
	book.Author = strings.TrimSpace(in.Author)
	book.Year = in.Year
	book.Genre = strings.TrimSpace(in.Genre)
	book.TotalCopies = in.TotalCopies
}

func matchesFilter(book models.Book, f models.BookFilter) bool {
	if f.Title != "" && !containsFold(book.Title, f.Title) {
		return false
	}
	if f.Author != "" && !containsFold(book.Author, f.Author) {
		return false
	}
	if f.Genre != "" && !strings.EqualFold(book.Genre, f.Genre) {
		return false
	}
	if f.Year != 0 && book.Year != f.Year {
		return false
	}
	return true
}

func sortedBooks(books map[int]models.Book, f models.BookFilter) []models.Book {
	result := make([]models.Book, 0, len(books))
	for _, book := range books {
		if matchesFilter(book, f) {
			result = append(result, book)
		}
	}
	sort.Slice(result, func(i, j int) bool { return result[i].ID < result[j].ID })
	return result
}

func containsFold(s, sub string) bool {
	return strings.Contains(strings.ToLower(s), strings.ToLower(sub))
}

func bookNotFound() error {
	return apperr.NotFound("BOOK_NOT_FOUND", "Книга не найдена")
}
