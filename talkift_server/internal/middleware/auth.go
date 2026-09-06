package middleware

import (
	"context"
	"encoding/json"
	"net/http"
	"strings"

	"talkift_server/internal/models"
	"talkift_server/internal/utils"
)

type contextKey string

const UserIDKey contextKey = "user_id"

func AuthMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		token := extractToken(r)
		if token == "" {
			writeJSON(w, http.StatusUnauthorized, models.ErrorPayload{
				Code:    models.CodeTokenInvalid,
				Message: models.ErrorMessage(models.CodeTokenInvalid),
			})
			return
		}

		claims, err := utils.ValidateToken(token)
		if err != nil {
			writeJSON(w, http.StatusUnauthorized, models.ErrorPayload{
				Code:    models.CodeTokenInvalid,
				Message: models.ErrorMessage(models.CodeTokenInvalid),
			})
			return
		}

		ctx := context.WithValue(r.Context(), UserIDKey, claims.UserID)
		next(w, r.WithContext(ctx))
	}
}

func extractToken(r *http.Request) string {
	authHeader := r.Header.Get("Authorization")
	if authHeader != "" {
		return strings.TrimPrefix(authHeader, "Bearer ")
	}

	return r.URL.Query().Get("token")
}

func GetUserID(r *http.Request) string {
	if v := r.Context().Value(UserIDKey); v != nil {
		if id, ok := v.(string); ok {
			return id
		}
	}
	return ""
}

func writeJSON(w http.ResponseWriter, statusCode int, data interface{}) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(statusCode)
	json.NewEncoder(w).Encode(data)
}
