package handlers

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"time"

	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

type ThirdPartyRegisterRequest struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

type ThirdPartyRegisterResponse struct {
	Success bool   `json:"success"`
	UUID    string `json:"uuid"`
	Token   string `json:"token"`
	Message string `json:"message"`
}

func (h *Handlers) HandleThirdPartyRegister(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		h.writeError(w, models.CodeMethodNotAllowed, http.StatusMethodNotAllowed)
		return
	}

	if !h.cfg.LoginServer.ThirdPartyEnabled {
		h.writeJSON(w, http.StatusForbidden, models.RegisterResponsePayload{
			Success: false,
			Code:    models.CodeThirdPartyUnavailable,
			Message: "third-party registration is disabled",
		})
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

	servers := h.cfg.LoginServer.ThirdPartyServers
	if len(servers) == 0 {
		h.writeJSON(w, http.StatusServiceUnavailable, models.RegisterResponsePayload{
			Success: false,
			Code:    models.CodeThirdPartyUnavailable,
			Message: "no third-party servers configured",
		})
		return
	}

	var lastErr error
	for _, serverURL := range servers {
		result, err := h.tryThirdPartyRegister(serverURL, req.Username, req.Password)
		if err != nil {
			lastErr = err
			utils.Warnf("third-party register attempt to %s failed: %v", serverURL, err)
			continue
		}

		if !result.Success {
			h.writeJSON(w, http.StatusConflict, models.RegisterResponsePayload{
				Success: false,
				Code:    models.CodeUsernameExists,
				Message: result.Message,
			})
			return
		}

		user := &models.User{
			ID:               result.UUID,
			Username:         req.Username,
			PasswordHash:     "",
			RegisterMethod:   string(models.RegisterMethodThirdParty),
			RegisterServerIP: serverURL,
			CreatedAt:        time.Now(),
			LastLoginAt:      time.Now(),
		}

		if err := h.store.SaveUser(user); err != nil {
			utils.Errorf("third-party register: save local user error: %v", err)
		}

		utils.Infof("third-party register success: user '%s' from %s", req.Username, serverURL)

		h.writeJSON(w, http.StatusCreated, models.RegisterResponsePayload{
			Success:  true,
			Code:     models.CodeSuccess,
			UUID:     result.UUID,
			Token:    result.Token,
			Username: req.Username,
		})
		return
	}

	utils.Errorf("third-party register: all servers failed, last error: %v", lastErr)
	h.writeJSON(w, http.StatusServiceUnavailable, models.RegisterResponsePayload{
		Success: false,
		Code:    models.CodeThirdPartyUnavailable,
		Message: "all third-party registration servers are unavailable",
	})
}

func (h *Handlers) tryThirdPartyRegister(serverURL, username, password string) (*ThirdPartyRegisterResponse, error) {
	reqBody := ThirdPartyRegisterRequest{
		Username: username,
		Password: password,
	}
	data, err := json.Marshal(reqBody)
	if err != nil {
		return nil, fmt.Errorf("marshal request: %w", err)
	}

	url := serverURL + "/api/register"
	client := &http.Client{Timeout: 10 * time.Second}
	resp, err := client.Post(url, "application/json", bytes.NewReader(data))
	if err != nil {
		return nil, fmt.Errorf("post to %s: %w", url, err)
	}
	defer resp.Body.Close()

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("read response: %w", err)
	}

	var result ThirdPartyRegisterResponse
	if err := json.Unmarshal(body, &result); err != nil {
		return nil, fmt.Errorf("unmarshal response: %w", err)
	}

	return &result, nil
}
