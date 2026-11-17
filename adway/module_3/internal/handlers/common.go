package handlers

import (
	"encoding/json"
	"errors"
	"io"
	"net/http"
	"strconv"
	"strings"

	"library-management-api/internal/apperr"
	"library-management-api/internal/response"
)

const maxBodyBytes = 1 << 20

func pathID(r *http.Request) (int, error) {
	id, err := strconv.Atoi(r.PathValue("id"))
	if err != nil || id <= 0 {
		return 0, apperr.BadRequest("INVALID_ID", "id должен быть положительным целым числом")
	}
	return id, nil
}

func decodeJSON(w http.ResponseWriter, r *http.Request, dst any) error {
	if ct := r.Header.Get("Content-Type"); ct != "" && !strings.HasPrefix(ct, "application/json") {
		return apperr.BadRequest("INVALID_CONTENT_TYPE", "Ожидается Content-Type: application/json")
	}
	dec := json.NewDecoder(http.MaxBytesReader(w, r.Body, maxBodyBytes))
	dec.DisallowUnknownFields()
	if err := dec.Decode(dst); err != nil {
		if errors.Is(err, io.EOF) {
			return apperr.BadRequest("EMPTY_BODY", "Тело запроса пустое")
		}
		return apperr.BadRequest("INVALID_JSON", "Некорректный JSON: "+err.Error())
	}
	if dec.More() {
		return apperr.BadRequest("INVALID_JSON", "В теле запроса должен быть один JSON-объект")
	}
	return nil
}

func NotFound(w http.ResponseWriter, r *http.Request) {
	response.Fail(w, http.StatusNotFound, "ROUTE_NOT_FOUND", "Маршрут "+r.URL.Path+" не найден")
}

func MethodNotAllowed(allowed string) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Allow", allowed)
		response.Fail(w, http.StatusMethodNotAllowed, "METHOD_NOT_ALLOWED",
			"Метод "+r.Method+" не поддерживается, допустимо: "+allowed)
	}
}
