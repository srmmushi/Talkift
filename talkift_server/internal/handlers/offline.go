package handlers

import (
	"encoding/json"
	"net/http"
	"time"

	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

func (h *Handlers) HandleOfflineLogin(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	if !h.cfg.LoginServer.Offline {
		h.writeJSON(w, http.StatusForbidden, models.LoginResponsePayload{
			Success: false,
			Code:    models.CodeUnauthorized,
			Message: "offline login is disabled",
		})
		return
	}

	var req LoginRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		h.writeError(w, models.CodeInvalidRequest, http.StatusBadRequest)
		return
	}

	if req.Username == "" {
		h.writeJSON(w, http.StatusBadRequest, models.LoginResponsePayload{
			Success: false,
			Code:    models.CodeInvalidRequest,
			Message: "username is required",
		})
		return
	}

	user, err := h.store.LoadUserByUsername(req.Username)
	if err != nil {
		user = &models.User{
			ID:             utils.NewUUID(),
			Username:       req.Username,
			PasswordHash:   "",
			RegisterMethod: string(models.RegisterMethodLocal),
			CreatedAt:      time.Now(),
			LastLoginAt:    time.Now(),
		}

		if saveErr := h.store.SaveUser(user); saveErr != nil {
			utils.Errorf("offline login: auto-create user error: %v", saveErr)
			h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
			return
		}

		utils.Infof("offline login: auto-created user '%s' (id=%s)", user.Username, user.ID)
	} else {
		user.LastLoginAt = time.Now()
		_ = h.store.SaveUser(user)
	}

	token, err := utils.GenerateToken(user.ID, user.Username, 7*24*time.Hour)
	if err != nil {
		utils.Errorf("offline login: generate token error: %v", err)
		h.writeError(w, models.CodeInternalError, http.StatusInternalServerError)
		return
	}

	utils.Infof("offline login success: user '%s' (id=%s)", user.Username, user.ID)

	h.writeJSON(w, http.StatusOK, models.LoginResponsePayload{
		Success:        true,
		Code:           models.CodeSuccess,
		UUID:           user.ID,
		Token:          token,
		Username:       user.Username,
		RegisterMethod: models.RegisterMethod(user.RegisterMethod),
	})
}
