package storage

import (
	"sync"

	"library-management-api/internal/models"
)

type Memory struct {
	Mu      sync.RWMutex
	Books   map[int]models.Book
	Readers map[int]models.Reader
	Loans   map[int]models.Loan

	nextBookID   int
	nextReaderID int
	nextLoanID   int
}

func NewMemory() *Memory {
	return &Memory{
		Books:        make(map[int]models.Book),
		Readers:      make(map[int]models.Reader),
		Loans:        make(map[int]models.Loan),
		nextBookID:   1,
		nextReaderID: 1,
		nextLoanID:   1,
	}
}

func (m *Memory) NextBookID() int {
	id := m.nextBookID
	m.nextBookID++
	return id
}

func (m *Memory) NextReaderID() int {
	id := m.nextReaderID
	m.nextReaderID++
	return id
}

func (m *Memory) NextLoanID() int {
	id := m.nextLoanID
	m.nextLoanID++
	return id
}
