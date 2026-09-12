package handler

import (
	"log"
	"net/http"
	"time"

	"official-server/internal/storage"
)

type LoginResponse struct {
	Code           int    `json:"code"`
	UUID           string `json:"uuid,omitempty"`
	Token          string `json:"token,omitempty"`
	Username       string `json:"username,omitempty"`
	Email          string `json:"email,omitempty"`
	RegisterMethod string `json:"register_method,omitempty"`
	Message        string `json:"message,omitempty"`
}

func HandleLogin(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()

		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, LoginResponse{Code: 1004, Message: "Method not allowed"})
			return
		}

		var req struct {
			Username string `json:"username"`
			Password string `json:"password"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, LoginResponse{Code: 1004, Message: "Invalid request body"})
			return
		}

		if req.Username == "" || req.Password == "" {
			writeJSON(w, http.StatusBadRequest, LoginResponse{Code: 1004, Message: "Username and password are required"})
			return
		}

		user, err := store.AuthenticateUser(req.Username, req.Password)
		if err != nil {
			code, msg, status := 1005, "Internal server error", http.StatusInternalServerError
			switch err {
			case storage.ErrUserNotFound:
				code, msg, status = 1003, "User not found", http.StatusNotFound
			case storage.ErrWrongPassword:
				code, msg, status = 1002, "Invalid password", http.StatusUnauthorized
			case storage.ErrUserBanned:
				code, msg, status = 1007, "User is banned", http.StatusForbidden
			}
			writeJSON(w, status, LoginResponse{Code: code, Message: msg})
			log.Printf("[LOGIN] FAIL username=%s ip=%s code=%d duration=%v", req.Username, r.RemoteAddr, code, time.Since(start))
			return
		}

		token := store.GetToken(user)
		log.Printf("[LOGIN] OK username=%s uuid=%s ip=%s duration=%v", req.Username, user.ID, r.RemoteAddr, time.Since(start))

		writeJSON(w, http.StatusOK, LoginResponse{
			Code: 0, UUID: user.ID, Token: token,
			Username: user.Username, Email: user.Email, RegisterMethod: user.RegisterMethod,
		})
	}
}

func HandleLoginEmail(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()

		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, LoginResponse{Code: 1004, Message: "Method not allowed"})
			return
		}

		var req struct {
			Email    string `json:"email"`
			Password string `json:"password"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, LoginResponse{Code: 1004, Message: "Invalid request body"})
			return
		}

		if req.Email == "" || req.Password == "" {
			writeJSON(w, http.StatusBadRequest, LoginResponse{Code: 1004, Message: "Email and password are required"})
			return
		}

		user, err := store.AuthenticateByEmail(req.Email, req.Password)
		if err != nil {
			code, msg, status := 1005, "Internal server error", http.StatusInternalServerError
			switch err {
			case storage.ErrUserNotFound:
				code, msg, status = 1003, "User not found", http.StatusNotFound
			case storage.ErrWrongPassword:
				code, msg, status = 1002, "Invalid password", http.StatusUnauthorized
			case storage.ErrUserBanned:
				code, msg, status = 1007, "User is banned", http.StatusForbidden
			}
			writeJSON(w, status, LoginResponse{Code: code, Message: msg})
			log.Printf("[LOGIN EMAIL] FAIL email=%s ip=%s code=%d duration=%v", req.Email, r.RemoteAddr, code, time.Since(start))
			return
		}

		token := store.GetToken(user)
		log.Printf("[LOGIN EMAIL] OK username=%s uuid=%s ip=%s duration=%v", user.Username, user.ID, r.RemoteAddr, time.Since(start))

		writeJSON(w, http.StatusOK, LoginResponse{
			Code: 0, UUID: user.ID, Token: token,
			Username: user.Username, Email: user.Email, RegisterMethod: user.RegisterMethod,
		})
	}
}
