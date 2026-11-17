package handlers

import (
	"net/http"

	"library-management-api/internal/models"
	"library-management-api/internal/response"
	"library-management-api/internal/services"
)

type LoanHandler struct {
	svc *services.LoanService
}

func NewLoanHandler(svc *services.LoanService) *LoanHandler {
	return &LoanHandler{svc: svc}
}

func (h *LoanHandler) List(w http.ResponseWriter, r *http.Request) {
	response.JSON(w, http.StatusOK, h.svc.GetAll())
}

func (h *LoanHandler) Active(w http.ResponseWriter, r *http.Request) {
	response.JSON(w, http.StatusOK, h.svc.Active())
}

func (h *LoanHandler) Get(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	loan, err := h.svc.GetByID(id)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, loan)
}

func (h *LoanHandler) Borrow(w http.ResponseWriter, r *http.Request) {
	var in models.LoanInput
	if err := decodeJSON(w, r, &in); err != nil {
		response.Error(w, err)
		return
	}
	loan, err := h.svc.Borrow(in)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusCreated, loan)
}

func (h *LoanHandler) Return(w http.ResponseWriter, r *http.Request) {
	id, err := pathID(r)
	if err != nil {
		response.Error(w, err)
		return
	}
	loan, err := h.svc.Return(id)
	if err != nil {
		response.Error(w, err)
		return
	}
	response.JSON(w, http.StatusOK, loan)
}

func (h *LoanHandler) Statistics(w http.ResponseWriter, r *http.Request) {
	response.JSON(w, http.StatusOK, h.svc.Statistics())
}
