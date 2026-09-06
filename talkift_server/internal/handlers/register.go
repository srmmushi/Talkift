package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

type RegisterRequest struct {
	Username         string                `json:"username"`
	Password         string                `json:"password"`
	RegisterMethod   models.RegisterMethod `json:"register_method"`
	RegisterServerIP string                `json:"register_server_ip,omitempty"`
}

func (h *Handlers) HandleRegister(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	var req RegisterRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		h.writeError(w, models.CodeInvalidRequest, http.StatusBadRequest)
		return
	}

	if req.Username == "" || req.Password == "" {
		h.writeJSON(w, http.StatusBadRequest, models.RegisterResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "username and password are required",
		})
		return
	}

	if len(req.Username) < 3 || len(req.Username) > 32 {
		h.writeJSON(w, http.StatusBadRequest, models.RegisterResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "username must be 3-32 characters",
		})
		return
	}

	if len(req.Password) < 6 {
		h.writeJSON(w, http.StatusBadRequest, models.RegisterResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "password must be at least 6 characters",
		})
		return
	}

	if req.RegisterMethod == "" {
		req.RegisterMethod = models.RegisterMethodLocal
	}

	existing, _ := h.store.LoadUserByUsername(req.Username)
	if existing != nil {
		h.writeJSON(w, http.StatusConflict, models.RegisterResponsePayload{
			Success: false,
			Code:    models.CodeUsernameExists,
			Message: models.ErrorMessage(models.CodeUsernameExists),
		})
		utils.Infof("register failed: username '%s' already exists", req.Username)
		return
	}

	hash, err := utils.HashPassword(req.Password)
	if err != nil {
		utils.Errorf("register: bcrypt hash error: %v", err)
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	user := &models.User{
		ID:               utils.NewUUID(),
		Username:         req.Username,
		PasswordHash:     hash,
		RegisterMethod:   string(req.RegisterMethod),
		RegisterServerIP: req.RegisterServerIP,
		CreatedAt:        time.Now(),
		LastLoginAt:      time.Now(),
	}

	if err := h.store.SaveUser(user); err != nil {
		utils.Errorf("register: save user error: %v", err)
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	token, err := utils.GenerateToken(user.ID, user.Username, 7*24*time.Hour)
	if err != nil {
		utils.Errorf("register: generate token error: %v", err)
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	utils.Infof("register success: user '%s' (id=%s, method=%s)", user.Username, user.ID, req.RegisterMethod)

	h.writeJSON(w, http.StatusCreated, models.RegisterResponsePayload{
		Success:  true,
		Code:     models.CodeSuccess,
		UUID:     user.ID,
		Token:    token,
		Username: user.Username,
	})
}
