package router

import (
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"library-management-api/internal/storage"
)

type envelope struct {
	StatusCode int             `json:"statusCode"`
	IsSuccess  bool            `json:"isSuccess"`
	Result     json.RawMessage `json:"result"`
	ErrorCode  *string         `json:"errorCode"`
}

func do(t *testing.T, h http.Handler, method, url, body string) (*httptest.ResponseRecorder, envelope) {
	t.Helper()
	req := httptest.NewRequest(method, url, strings.NewReader(body))
	req.Header.Set("Content-Type", "application/json")
	rec := httptest.NewRecorder()
	h.ServeHTTP(rec, req)
	var env envelope
	if err := json.Unmarshal(rec.Body.Bytes(), &env); err != nil {
		t.Fatalf("ответ не JSON: %s", rec.Body.String())
	}
	return rec, env
}

func TestCreateAndGetBook(t *testing.T) {
	h := New(storage.NewMemory())
	rec, env := do(t, h, http.MethodPost, "/api/books",
		`{"title":"Go","author":"Pike","year":2015,"genre":"IT","totalCopies":2}`)
	if rec.Code != http.StatusCreated || !env.IsSuccess || env.StatusCode != 201 {
		t.Fatalf("создание книги: %d %s", rec.Code, rec.Body.String())
	}
	rec, env = do(t, h, http.MethodGet, "/api/books/1", "")
	if rec.Code != http.StatusOK || !strings.Contains(string(env.Result), `"availableCopies": 2`) {
		t.Fatalf("получение книги: %d %s", rec.Code, rec.Body.String())
	}
}

func TestInvalidJSONAndValidation(t *testing.T) {
	h := New(storage.NewMemory())
	rec, env := do(t, h, http.MethodPost, "/api/books", `{"title": "Go",`)
	if rec.Code != http.StatusBadRequest || env.ErrorCode == nil || *env.ErrorCode != "INVALID_JSON" {
		t.Fatalf("битый JSON: %d %s", rec.Code, rec.Body.String())
	}
	rec, env = do(t, h, http.MethodPost, "/api/readers", `{"firstName":"A","lastName":"B","email":"bad","age":20}`)
	if rec.Code != http.StatusBadRequest || *env.ErrorCode != "VALIDATION_ERROR" {
		t.Fatalf("валидация: %d %s", rec.Code, rec.Body.String())
	}
}

func TestUnknownRouteAndMethod(t *testing.T) {
	h := New(storage.NewMemory())
	if rec, _ := do(t, h, http.MethodGet, "/api/unknown", ""); rec.Code != http.StatusNotFound {
		t.Fatalf("неизвестный маршрут: %d", rec.Code)
	}
	rec, env := do(t, h, http.MethodPatch, "/api/books", "")
	if rec.Code != http.StatusMethodNotAllowed || *env.ErrorCode != "METHOD_NOT_ALLOWED" || rec.Header().Get("Allow") == "" {
		t.Fatalf("неподдерживаемый метод: %d %s", rec.Code, rec.Body.String())
	}
}

func TestBorrowForMissingReader(t *testing.T) {
	h := New(storage.NewMemory())
	do(t, h, http.MethodPost, "/api/books", `{"title":"Go","author":"Pike","year":2015,"genre":"IT","totalCopies":1}`)
	rec, env := do(t, h, http.MethodPost, "/api/loans", `{"bookId":1,"readerId":99}`)
	if rec.Code != http.StatusNotFound || *env.ErrorCode != "READER_NOT_FOUND" {
		t.Fatalf("выдача несуществующему читателю: %d %s", rec.Code, rec.Body.String())
	}
}
