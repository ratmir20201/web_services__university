package router

import (
	"net/http"

	"library-management-api/internal/handlers"
	"library-management-api/internal/middleware"
	"library-management-api/internal/services"
	"library-management-api/internal/storage"
)

func New(store *storage.Memory) http.Handler {
	books := handlers.NewBookHandler(services.NewBookService(store))
	readers := handlers.NewReaderHandler(services.NewReaderService(store))
	loans := handlers.NewLoanHandler(services.NewLoanService(store))

	mux := http.NewServeMux()

	mux.HandleFunc("GET /api/books", books.List)
	mux.HandleFunc("POST /api/books", books.Create)
	mux.HandleFunc("GET /api/books/search", books.Search)
	mux.HandleFunc("GET /api/books/{id}", books.Get)
	mux.HandleFunc("PUT /api/books/{id}", books.Update)
	mux.HandleFunc("DELETE /api/books/{id}", books.Delete)

	mux.HandleFunc("GET /api/readers", readers.List)
	mux.HandleFunc("POST /api/readers", readers.Create)
	mux.HandleFunc("GET /api/readers/{id}", readers.Get)
	mux.HandleFunc("PUT /api/readers/{id}", readers.Update)
	mux.HandleFunc("DELETE /api/readers/{id}", readers.Delete)
	mux.HandleFunc("GET /api/readers/{id}/loans", readers.Loans)

	mux.HandleFunc("GET /api/loans", loans.List)
	mux.HandleFunc("POST /api/loans", loans.Borrow)
	mux.HandleFunc("GET /api/loans/active", loans.Active)
	mux.HandleFunc("GET /api/loans/{id}", loans.Get)
	mux.HandleFunc("PUT /api/loans/{id}/return", loans.Return)

	mux.HandleFunc("GET /api/statistics", loans.Statistics)

	notAllowed := map[string]string{
		"/api/books":              "GET, POST",
		"/api/books/{id}":         "GET, PUT, DELETE",
		"/api/readers":            "GET, POST",
		"/api/readers/{id}":       "GET, PUT, DELETE",
		"/api/readers/{id}/loans": "GET",
		"/api/loans":              "GET, POST",
		"/api/loans/{id}":         "GET",
		"/api/loans/{id}/return":  "PUT",
		"/api/statistics":         "GET",
	}
	for pattern, allowed := range notAllowed {
		mux.HandleFunc(pattern, handlers.MethodNotAllowed(allowed))
	}
	mux.HandleFunc("/", handlers.NotFound)

	return middleware.Recover(middleware.Logging(mux))
}
