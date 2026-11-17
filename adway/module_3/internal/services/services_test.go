package services

import (
	"errors"
	"net/http"
	"testing"
	"time"

	"library-management-api/internal/apperr"
	"library-management-api/internal/models"
	"library-management-api/internal/storage"
)

func setup(t *testing.T, copies int) (*LoanService, *models.Book, *models.Reader) {
	t.Helper()
	store := storage.NewMemory()
	book, err := NewBookService(store).Create(models.BookInput{Title: "Go", Author: "Pike", Year: 2015, Genre: "IT", TotalCopies: copies})
	if err != nil {
		t.Fatal(err)
	}
	reader, err := NewReaderService(store).Create(models.ReaderInput{FirstName: "Ivan", LastName: "Petrov", Email: "ivan@test.kz", Age: 20})
	if err != nil {
		t.Fatal(err)
	}
	return NewLoanService(store), book, reader
}

func assertCode(t *testing.T, err error, status int, code string) {
	t.Helper()
	var appErr *apperr.Error
	if !errors.As(err, &appErr) || appErr.Status != status || appErr.Code != code {
		t.Fatalf("ожидалась ошибка %d %s, получено %v", status, code, err)
	}
}

func TestBorrowAndReturnChangesAvailableCopies(t *testing.T) {
	loans, book, reader := setup(t, 1)
	loan, err := loans.Borrow(models.LoanInput{BookID: book.ID, ReaderID: reader.ID})
	if err != nil {
		t.Fatal(err)
	}
	if b := loans.store.Books[book.ID]; b.AvailableCopies != 0 {
		t.Fatalf("после выдачи AvailableCopies = %d, ожидалось 0", b.AvailableCopies)
	}
	_, err = loans.Borrow(models.LoanInput{BookID: book.ID, ReaderID: reader.ID})
	assertCode(t, err, http.StatusConflict, "BOOK_NOT_AVAILABLE")

	if _, err := loans.Return(loan.ID); err != nil {
		t.Fatal(err)
	}
	if b := loans.store.Books[book.ID]; b.AvailableCopies != 1 {
		t.Fatalf("после возврата AvailableCopies = %d, ожидалось 1", b.AvailableCopies)
	}
	_, err = loans.Return(loan.ID)
	assertCode(t, err, http.StatusConflict, "LOAN_ALREADY_RETURNED")
}

func TestActiveLoansLimit(t *testing.T) {
	loans, book, reader := setup(t, 10)
	for i := 0; i < MaxActiveLoans; i++ {
		if _, err := loans.Borrow(models.LoanInput{BookID: book.ID, ReaderID: reader.ID}); err != nil {
			t.Fatal(err)
		}
	}
	_, err := loans.Borrow(models.LoanInput{BookID: book.ID, ReaderID: reader.ID})
	assertCode(t, err, http.StatusConflict, "LOAN_LIMIT_EXCEEDED")
}

func TestDuplicateEmailAndDeleteWithActiveLoan(t *testing.T) {
	loans, book, reader := setup(t, 2)
	readers := NewReaderService(loans.store)
	_, err := readers.Create(models.ReaderInput{FirstName: "A", LastName: "B", Email: "IVAN@test.kz", Age: 30})
	assertCode(t, err, http.StatusConflict, "EMAIL_ALREADY_EXISTS")

	if _, err := loans.Borrow(models.LoanInput{BookID: book.ID, ReaderID: reader.ID}); err != nil {
		t.Fatal(err)
	}
	assertCode(t, readers.Delete(reader.ID), http.StatusConflict, "READER_HAS_ACTIVE_LOANS")
	assertCode(t, NewBookService(loans.store).Delete(book.ID), http.StatusConflict, "BOOK_HAS_ACTIVE_LOANS")
}

func TestStatisticsIsCalculated(t *testing.T) {
	loans, book, reader := setup(t, 3)
	past := time.Date(time.Now().Year(), time.March, 1, 10, 0, 0, 0, time.UTC)
	loans.now = func() time.Time { return past }
	if _, err := loans.Borrow(models.LoanInput{BookID: book.ID, ReaderID: reader.ID, Days: 7}); err != nil {
		t.Fatal(err)
	}
	loans.now = func() time.Time { return past.AddDate(0, 0, 10) }
	stats := loans.Statistics()
	if stats.ActiveLoans != 1 || stats.OverdueLoans != 1 {
		t.Fatalf("активных %d, просроченных %d, ожидалось 1 и 1", stats.ActiveLoans, stats.OverdueLoans)
	}
	if stats.AvailableCopies != 2 || stats.TotalCopies != 3 || stats.AvailabilityPercent != 66.7 {
		t.Fatalf("неверные экземпляры: %+v", stats)
	}
	if stats.LoansByMonth[2].Loans != 1 || len(stats.TopBooks) != 1 {
		t.Fatalf("неверная статистика по месяцам или топ: %+v", stats)
	}
}
