package handler

import (
	"encoding/json"
	"log"
	"net/http"
	"time"

	"loginserver/internal/storage"
)

type RegisterRequest struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

type RegisterResponse struct {
	Code int    `json:"code"`
	UUID string `json:"uuid,omitempty"`
	Token string `json:"token,omitempty"`
	Message string `json:"message,omitempty"`
}

func HandleRegister(store *storage.Storage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()

		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, RegisterResponse{
				Code: 1004, Message: "Method not allowed",
			})
			return
		}

		var req RegisterRequest
		if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{
				Code: 1004, Message: "Invalid request body",
			})
			return
		}

		if req.Username == "" || req.Password == "" {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{
				Code: 1004, Message: "Username and password are required",
			})
			return
		}

		if len(req.Username) < 3 || len(req.Username) > 32 {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{
				Code: 1004, Message: "Username must be 3-32 characters",
			})
			return
		}

		if len(req.Password) < 6 {
			writeJSON(w, http.StatusBadRequest, RegisterResponse{
				Code: 1004, Message: "Password must be at least 6 characters",
			})
			return
		}

		user, err := store.CreateUser(req.Username, req.Password)
		if err != nil {
			code := 1005
			msg := "Internal server error"
			status := http.StatusInternalServerError

			if err == storage.ErrUsernameExists {
				code = 1001
				msg = "Username already exists"
				status = http.StatusConflict
			}

			writeJSON(w, status, RegisterResponse{Code: code, Message: msg})
			log.Printf("[REGISTER] FAIL username=%s ip=%s code=%d duration=%v",
				req.Username, r.RemoteAddr, code, time.Since(start))
			return
		}

		token := generateToken(user.ID)

		log.Printf("[REGISTER] OK username=%s uuid=%s ip=%s duration=%v",
			req.Username, user.ID, r.RemoteAddr, time.Since(start))

		writeJSON(w, http.StatusCreated, RegisterResponse{
			Code: 0,
			UUID: user.ID,
			Token: token,
		})
	}
}
