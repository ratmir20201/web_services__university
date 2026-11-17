package response

import (
	"encoding/json"
	"errors"
	"log"
	"net/http"

	"library-management-api/internal/apperr"
)

type Envelope struct {
	StatusCode   int     `json:"statusCode"`
	IsSuccess    bool    `json:"isSuccess"`
	Result       any     `json:"result"`
	ErrorCode    *string `json:"errorCode"`
	ErrorMessage *string `json:"errorMessage"`
}

func JSON(w http.ResponseWriter, status int, result any) {
	write(w, status, Envelope{StatusCode: status, IsSuccess: true, Result: result})
}

func Fail(w http.ResponseWriter, status int, code, message string) {
	write(w, status, Envelope{StatusCode: status, ErrorCode: &code, ErrorMessage: &message})
}

func Error(w http.ResponseWriter, err error) {
	var appErr *apperr.Error
	if errors.As(err, &appErr) {
		Fail(w, appErr.Status, appErr.Code, appErr.Message)
		return
	}
	log.Printf("internal error: %v", err)
	Fail(w, http.StatusInternalServerError, "INTERNAL_ERROR", "Внутренняя ошибка сервера")
}

func write(w http.ResponseWriter, status int, body Envelope) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	enc := json.NewEncoder(w)
	enc.SetIndent("", "  ")
	if err := enc.Encode(body); err != nil {
		log.Printf("encode response: %v", err)
	}
}
