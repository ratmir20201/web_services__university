package main

import (
	"log"
	"net/http"
	"time"

	"library-management-api/internal/models"
	"library-management-api/internal/router"
	"library-management-api/internal/services"
	"library-management-api/internal/storage"
)

const addr = ":8080"

func main() {
	store := storage.NewMemory()
	seed(store)

	server := &http.Server{
		Addr:              addr,
		Handler:           router.New(store),
		ReadHeaderTimeout: 5 * time.Second,
	}
	log.Printf("Library Management API запущен на http://localhost%s", addr)
	if err := server.ListenAndServe(); err != nil {
		log.Fatal(err)
	}
}

func seed(store *storage.Memory) {
	books := services.NewBookService(store)
	readers := services.NewReaderService(store)
	initialBooks := []models.BookInput{
		{Title: "Чистый код", Author: "Robert Martin", Year: 2008, Genre: "IT", TotalCopies: 3},
		{Title: "Чистая архитектура", Author: "Robert Martin", Year: 2017, Genre: "IT", TotalCopies: 2},
		{Title: "Мастер и Маргарита", Author: "Михаил Булгаков", Year: 1967, Genre: "Роман", TotalCopies: 4},
		{Title: "Совершенный код", Author: "Steve McConnell", Year: 2004, Genre: "IT", TotalCopies: 1},
	}
	for _, b := range initialBooks {
		if _, err := books.Create(b); err != nil {
			log.Fatalf("seed book: %v", err)
		}
	}
	initialReaders := []models.ReaderInput{
		{FirstName: "Айдар", LastName: "Нурланов", Email: "aidar@example.com", Age: 20},
		{FirstName: "Мария", LastName: "Иванова", Email: "maria@example.com", Age: 22},
	}
	for _, r := range initialReaders {
		if _, err := readers.Create(r); err != nil {
			log.Fatalf("seed reader: %v", err)
		}
	}
}
