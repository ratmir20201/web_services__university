package handlers

import (
	"net/http"

	"library-management-api/internal/models"
	"library-management-api/internal/response"
	"library-management-api/internal/services"
)

type ReaderHandler struct {
	svc *services.ReaderService
}

func NewReaderHandler(svc *services.ReaderService) *ReaderHandler {
	return &ReaderHandler{svc: svc}
}

func (h *ReaderHandler) List(w http.ResponseWriter, r *http.Request) {
	response.JSON(w, http.StatusOK, h.svc.GetAll(r.URL.Query().Get("name")))
}

func (h *ReaderHandler) Get(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	reader, err := h.svc.GetByID(id)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, reader)
}

func (h *ReaderHandler) Create(w http.ResponseWriter, r *http.Request) {
	var in models.ReaderInput
	if err := decodeJSON(w, r, &in); err != nil {
		response.Error(w, err)
		return
	}
	reader, err := h.svc.Create(in)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusCreated, reader)
}

func (h *ReaderHandler) Update(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	var in models.ReaderInput
	if err := decodeJSON(w, r, &in); err != nil {
		response.Error(w, err)
		return
	}
	reader, err := h.svc.Update(id, in)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, reader)
}

func (h *ReaderHandler) Delete(w http.ResponseWriter, r *http.Request) {
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

func (h *ReaderHandler) Loans(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	loans, err := h.svc.Loans(id)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, loans)
}
