package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

type LoginRequest struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

func (h *Handlers) HandleLogin(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	var req LoginRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		h.writeError(w, models.CodeInvalidRequest, http.StatusBadRequest)
		return
	}

	if req.Username == "" || req.Password == "" {
		h.writeJSON(w, http.StatusBadRequest, models.LoginResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "username and password are required",
		})
		return
	}

	user, err := h.store.LoadUserByUsername(req.Username)
	if err != nil {
		utils.Infof("login failed: user '%s' not found", req.Username)
		h.writeJSON(w, http.StatusUnauthorized, models.LoginResponsePayload{
			Success: false,
			Code:    models.CodeUserNotFound,
			Message: models.ErrorMessage(models.CodeUserNotFound),
		})
		return
	}

	if !utils.CheckPassword(req.Password, user.PasswordHash) {
		utils.Infof("login failed: wrong password for user '%s'", req.Username)
		h.writeJSON(w, http.StatusUnauthorized, models.LoginResponsePayload{
			Success: false,
			Code:    models.CodePasswordError,
			Message: models.ErrorMessage(models.CodePasswordError),
		})
		return
	}

	user.LastLoginAt = time.Now()
	if err := h.store.SaveUser(user); err != nil {
		utils.Errorf("login: update last_login error: %v", err)
	}

	token, err := utils.GenerateToken(user.ID, user.Username, 7*24*time.Hour)
	if err != nil {
		utils.Errorf("login: generate token error: %v", err)
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	utils.Infof("login success: user '%s' (id=%s)", user.Username, user.ID)

	h.writeJSON(w, http.StatusOK, models.LoginResponsePayload{
		Success:        true,
		Code:           models.CodeSuccess,
		UUID:           user.ID,
		Token:          token,
		Username:       user.Username,
		RegisterMethod: models.RegisterMethod(user.RegisterMethod),
	})
}
