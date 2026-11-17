package handlers

import (
	"net/http"
	"strconv"

	"library-management-api/internal/apperr"
	"library-management-api/internal/models"
	"library-management-api/internal/response"
	"library-management-api/internal/services"
)

type BookHandler struct {
	svc *services.BookService
}

func NewBookHandler(svc *services.BookService) *BookHandler {
	return &BookHandler{svc: svc}
}

func (h *BookHandler) List(w http.ResponseWriter, r *http.Request) {
	response.JSON(w, http.StatusOK, h.svc.GetAll())
}

func (h *BookHandler) Get(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	book, err := h.svc.GetByID(id)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, book)
}

func (h *BookHandler) Search(w http.ResponseWriter, r *http.Request) {
	q := r.URL.Query()
	filter := models.BookFilter{Title: q.Get("title"), Author: q.Get("author"), Genre: q.Get("genre")}
	if y := q.Get("year"); y != "" {
		year, err := strconv.Atoi(y)
		if err != nil {
			response.Error(w, apperr.BadRequest("INVALID_QUERY", "year должен быть числом"))
			return
		}
		filter.Year = year
	}
	response.JSON(w, http.StatusOK, h.svc.Search(filter))
}

func (h *BookHandler) Create(w http.ResponseWriter, r *http.Request) {
	var in models.BookInput
	if err := decodeJSON(w, r, &in); err != nil {
		response.Error(w, err)
		return
	}
	book, err := h.svc.Create(in)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusCreated, book)
}

func (h *BookHandler) Update(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	var in models.BookInput
	if err := decodeJSON(w, r, &in); err != nil {
		response.Error(w, err)
		return
	}
	book, err := h.svc.Update(id, in)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, book)
}

func (h *BookHandler) Delete(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	if err := h.svc.Delete(id); err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, map[string]int{"deletedId": id})
}
