package models

import "time"

type Loan struct {
	ID         int        `json:"id"`
	BookID     int        `json:"bookId"`
	ReaderID   int        `json:"readerId"`
	BorrowedAt time.Time  `json:"borrowedAt"`
	DueDate    time.Time  `json:"dueDate"`
	ReturnedAt *time.Time `json:"returnedAt"`
	IsReturned bool       `json:"isReturned"`
}

type LoanInput struct {
	BookID   int `json:"bookId"`
	ReaderID int `json:"readerId"`
	Days     int `json:"days"`
}

type TopBook struct {
	BookID int    `json:"bookId"`
	Title  string `json:"title"`
	Loans  int    `json:"loans"`
}

type MonthStat struct {
	Month string `json:"month"`
	Loans int    `json:"loans"`
}

type Statistics struct {
	TotalBooks          int         `json:"totalBooks"`
	TotalCopies         int         `json:"totalCopies"`
	AvailableCopies     int         `json:"availableCopies"`
	AvailabilityPercent float64     `json:"availabilityPercent"`
	TotalReaders        int         `json:"totalReaders"`
	ActiveLoans         int         `json:"activeLoans"`
	OverdueLoans        int         `json:"overdueLoans"`
	TopBooks            []TopBook   `json:"topBooks"`
	LoansByMonth        []MonthStat `json:"loansByMonth"`
}
