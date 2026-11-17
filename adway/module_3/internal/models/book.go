package models

type Book struct {
	ID              int    `json:"id"`
	Title           string `json:"title"`
	Author          string `json:"author"`
	Year            int    `json:"year"`
	Genre           string `json:"genre"`
	TotalCopies     int    `json:"totalCopies"`
	AvailableCopies int    `json:"availableCopies"`
}

type BookInput struct {
	Title       string `json:"title"`
	Author      string `json:"author"`
	Year        int    `json:"year"`
	Genre       string `json:"genre"`
	TotalCopies int    `json:"totalCopies"`
}

type BookFilter struct {
	Title  string
	Author string
	Genre  string
	Year   int
}
