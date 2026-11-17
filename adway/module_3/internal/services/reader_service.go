package services

import (
	"sort"
	"strings"

	"library-management-api/internal/apperr"
	"library-management-api/internal/models"
	"library-management-api/internal/storage"
)

type ReaderService struct {
	store *storage.Memory
}

func NewReaderService(store *storage.Memory) *ReaderService {
	return &ReaderService{store: store}
}

func (s *ReaderService) GetAll(name string) []models.Reader {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	result := make([]models.Reader, 0, len(s.store.Readers))
	for _, r := range s.store.Readers {
		if name == "" || containsFold(r.FirstName+" "+r.LastName, name) {
			result = append(result, r)
		}
	}
	sort.Slice(result, func(i, j int) bool { return result[i].ID < result[j].ID })
	return result
}

func (s *ReaderService) GetByID(id int) (*models.Reader, error) {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	reader, ok := s.store.Readers[id]
	if !ok {
		return nil, readerNotFound()
	}
	return &reader, nil
}

func (s *ReaderService) Create(in models.ReaderInput) (*models.Reader, error) {
	if err := validateReader(in); err != nil {
		return nil, err
	}
	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	if emailTaken(s.store.Readers, in.Email, 0) {
		return nil, emailConflict()
	}
	reader := models.Reader{ID: s.store.NextReaderID(), IsActive: true}
	applyReaderInput(&reader, in)
	s.store.Readers[reader.ID] = reader
	return &reader, nil
}

func (s *ReaderService) Update(id int, in models.ReaderInput) (*models.Reader, error) {
	if err := validateReader(in); err != nil {
		return nil, err
	}
	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	reader, ok := s.store.Readers[id]
	if !ok {
		return nil, readerNotFound()
	}
	if emailTaken(s.store.Readers, in.Email, id) {
		return nil, emailConflict()
	}
	applyReaderInput(&reader, in)
	s.store.Readers[id] = reader
	return &reader, nil
}

func (s *ReaderService) Delete(id int) error {
	s.store.Mu.Lock()
	defer s.store.Mu.Unlock()
	if _, ok := s.store.Readers[id]; !ok {
		return readerNotFound()
	}
	if countActiveLoans(s.store.Loans, id) > 0 {
		return apperr.Conflict("READER_HAS_ACTIVE_LOANS", "У читателя есть активные выдачи")
	}
	delete(s.store.Readers, id)
	return nil
}

func (s *ReaderService) Loans(id int) ([]models.Loan, error) {
	s.store.Mu.RLock()
	defer s.store.Mu.RUnlock()
	if _, ok := s.store.Readers[id]; !ok {
		return nil, readerNotFound()
	}
	return filterLoans(s.store.Loans, func(l models.Loan) bool { return l.ReaderID == id }), nil
}

func validateReader(in models.ReaderInput) error {
	switch {
	case strings.TrimSpace(in.FirstName) == "":
		return apperr.BadRequest("VALIDATION_ERROR", "Поле firstName обязательно")
	case strings.TrimSpace(in.LastName) == "":
		return apperr.BadRequest("VALIDATION_ERROR", "Поле lastName обязательно")
	case !isValidEmail(in.Email):
		return apperr.BadRequest("VALIDATION_ERROR", "Некорректный email")
	case in.Age < 6 || in.Age > 120:
		return apperr.BadRequest("VALIDATION_ERROR", "Возраст должен быть от 6 до 120")
	}
	return nil
}

func isValidEmail(email string) bool {
	at := strings.Index(email, "@")
	return at > 0 && strings.Contains(email[at:], ".") && !strings.ContainsAny(email, " ,")
}

func emailTaken(readers map[int]models.Reader, email string, exceptID int) bool {
	for id, r := range readers {
		if id != exceptID && strings.EqualFold(r.Email, strings.TrimSpace(email)) {
			return true
		}
	}
	return false
}

func applyReaderInput(reader *models.Reader, in models.ReaderInput) {
	reader.FirstName = strings.TrimSpace(in.FirstName)
	reader.LastName = strings.TrimSpace(in.LastName)
	reader.Email = strings.ToLower(strings.TrimSpace(in.Email))
	reader.Age = in.Age
	if in.IsActive != nil {
		reader.IsActive = *in.IsActive
	}
}

func readerNotFound() error {
	return apperr.NotFound("READER_NOT_FOUND", "Читатель не найден")
}

func emailConflict() error {
	return apperr.Conflict("EMAIL_ALREADY_EXISTS", "Читатель с таким email уже зарегистрирован")
}
