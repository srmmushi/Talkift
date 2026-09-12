package handler

import (
	"encoding/json"
	"log"
	"net/http"
	"time"

	"official-server/internal/storage"
)

type LoginRequest struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

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
			writeJSON(w, http.StatusMethodNotAllowed, LoginResponse{
				Code: 1004, Message: "Method not allowed",
			})
			return
		}

		var req LoginRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeJSON(w, http.StatusBadRequest, LoginResponse{
				Code: 1004, Message: "Invalid request body",
			})
			return
		}

		if req.Username == "" || req.Password == "" {
			writeJSON(w, http.StatusBadRequest, LoginResponse{
				Code: 1004, Message: "Username and password are required",
			})
			return
		}

		user, err := store.AuthenticateUser(req.Username, req.Password)
		if err != nil {
			code := 1005
			msg := "Internal server error"
			status := http.StatusInternalServerError

			switch err {
			case storage.ErrUserNotFound:
				code = 1003
				msg = "User not found"
				status = http.StatusNotFound
			case storage.ErrWrongPassword:
				code = 1002
				msg = "Invalid password"
				status = http.StatusUnauthorized
			}

			writeJSON(w, status, LoginResponse{Code: code, Message: msg})
			log.Printf("[LOGIN] FAIL username=%s ip=%s code=%d duration=%v",
				req.Username, r.RemoteAddr, code, time.Since(start))
			return
		}

		token := generateToken(user.ID)

		log.Printf("[LOGIN] OK username=%s uuid=%s ip=%s duration=%v",
			req.Username, user.ID, r.RemoteAddr, time.Since(start))

		writeJSON(w, http.StatusOK, LoginResponse{
			Code:           0,
			UUID:           user.ID,
			Token:          token,
			Username:       user.Username,
			Email:          user.Email,
			RegisterMethod: user.RegisterMethod,
		})
	}
}
