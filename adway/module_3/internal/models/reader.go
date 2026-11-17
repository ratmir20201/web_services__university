package models

type Reader struct {
	ID        int    `json:"id"`
	FirstName string `json:"firstName"`
	LastName  string `json:"lastName"`
	Email     string `json:"email"`
	Age       int    `json:"age"`
	IsActive  bool   `json:"isActive"`
}

type ReaderInput struct {
	FirstName string `json:"firstName"`
	LastName  string `json:"lastName"`
	Email     string `json:"email"`
	Age       int    `json:"age"`
	IsActive  *bool  `json:"isActive"`
}
