package handler

import (
	"log"
	"net/http"
	"time"

	"official-server/internal/storage"
)

type RegisterResponse struct {
	Code           int    `json:"code"`
	UUID           string `json:"uuid,omitempty"`
	Token          string `json:"token,omitempty"`
	Username       string `json:"username,omitempty"`
	RegisterMethod string `json:"register_method,omitempty"`
	Message        string `json:"message,omitempty"`
}

func HandleRegister(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()

		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, RegisterResponse{Code: 1004, Message: "Method not allowed"})
			return
		}

		var req struct {
			Username       string `json:"username"`
			Password       string `json:"password"`
			Email          string `json:"email,omitempty"`
			RegisterMethod string `json:"register_method,omitempty"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{Code: 1004, Message: "Invalid request body"})
			return
		}

		if req.Username == "" || req.Password == "" {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{Code: 1004, Message: "Username and password are required"})
			return
		}
		if len(req.Username) < 3 || len(req.Username) > 32 {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{Code: 1004, Message: "Username must be 3-32 characters"})
			return
		}
		if len(req.Password) < 6 {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{Code: 1004, Message: "Password must be at least 6 characters"})
			return
		}

		method := req.RegisterMethod
		if method == "" {
			method = "official"
		}

		user, err := store.CreateUser(req.Username, req.Password, req.Email, method)
		if err != nil {
			code, msg, status := 1005, "Internal server error", http.StatusInternalServerError
			switch err {
			case storage.ErrUsernameExists:
				code, msg, status = 1001, "Username already exists", http.StatusConflict
			case storage.ErrEmailExists:
				code, msg, status = 1006, "Email already registered", http.StatusConflict
			}
			writeJSON(w, status, RegisterResponse{Code: code, Message: msg})
			log.Printf("[REGISTER] FAIL username=%s ip=%s code=%d duration=%v", req.Username, r.RemoteAddr, code, time.Since(start))
			return
		}

		token := store.GetToken(user)
		log.Printf("[REGISTER] OK username=%s uuid=%s method=%s ip=%s duration=%v", req.Username, user.ID, method, r.RemoteAddr, time.Since(start))

		writeJSON(w, http.StatusCreated, RegisterResponse{
			Code: 0, UUID: user.ID, Token: token,
			Username: user.Username, RegisterMethod: method,
		})
	}
}
