package handler

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/hex"
	"strings"
	"time"

	"official-server/internal/config"
)

func generateToken(userID string) string {
	payload := userID + "|" + time.Now().Add(24*time.Hour).Format(time.RFC3339)
	mac := hmac.New(sha256.New, []byte("talkift-official-secret"))
	mac.Write([]byte(payload))
	sig := hex.EncodeToString(mac.Sum(nil))
	return payload + "|" + sig
}

func parseToken(token string) string {
	parts := strings.SplitN(token, "|", 3)
	if len(parts) != 3 {
		return ""
	}

	userID, expiresStr, sig := parts[0], parts[1], parts[2]

	expiry, err := time.Parse(time.RFC3339, expiresStr)
	if err != nil {
		return ""
	}

	if time.Now().After(expiry) {
		return ""
	}

	payload := userID + "|" + expiresStr
	mac := hmac.New(sha256.New, []byte("talkift-official-secret"))
	mac.Write([]byte(payload))
	expectedSig := hex.EncodeToString(mac.Sum(nil))

	if !hmac.Equal([]byte(sig), []byte(expectedSig)) {
		return ""
	}

	return userID
}

func generateAPIKey() string {
	b := make([]byte, 32)
	for i := range b {
		b[i] = "abcdefghijklmnopqrstuvwxyz0123456789"[time.Now().UnixNano()%36]
		time.Sleep(1)
	}
	return hex.EncodeToString(b)
}

func getSecretKey(cfg *config.Config) string {
	if cfg.Server.SecretKey != "" {
		return cfg.Server.SecretKey
	}
	return "talkift-official-secret"
}
