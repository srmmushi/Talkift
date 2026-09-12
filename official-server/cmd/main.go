package main

import (
	"context"
	"flag"
	"fmt"
	"log"
	"net/http"
	"os"
	"os/signal"
	"strings"
	"syscall"
	"time"

	"official-server/internal/config"
	"official-server/internal/handler"
	"official-server/internal/storage"
)

func main() {
	port := flag.Int("port", 0, "Server port (overrides config.toml)")
	flag.Parse()

	cfg, err := config.Load()
	if err != nil {
		log.Fatalf("Failed to load config: %v", err)
	}

	if *port > 0 {
		cfg.Server.Port = *port
	}

	config.EnsureDirs(cfg)

	logFile, err := os.OpenFile(cfg.Log.Path, os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0644)
	if err != nil {
		log.Printf("Warning: Cannot open log file %s: %v", cfg.Log.Path, err)
	} else {
		defer logFile.Close()
		log.SetOutput(os.Stdout)
	}

	store := storage.New(cfg.Storage.Path)
	versionStore := storage.NewVersionStorage(cfg.Storage.VersionPath, cfg.Storage.ConfigPath)

	mux := http.NewServeMux()

	// Auth
	mux.HandleFunc("/api/register", handler.HandleRegister(store))
	mux.HandleFunc("/api/login", handler.HandleLogin(store))
	mux.HandleFunc("/api/login/email", handler.HandleLoginEmail(store))
	mux.HandleFunc("/api/verify", handler.HandleVerify(store))
	mux.HandleFunc("/api/logout", handler.HandleLogout(store))

	// User
	mux.HandleFunc("/api/user/profile", handler.HandleGetProfile(store))
	mux.HandleFunc("/api/user/profile/update", handler.HandleUpdateProfile(store))
	mux.HandleFunc("/api/user/status", handler.HandleSetStatus(store))
	mux.HandleFunc("/api/user/search", handler.HandleSearchUsers(store))
	mux.HandleFunc("/api/user/change-password", handler.HandleChangePassword(store))
	mux.HandleFunc("/api/user/avatar", handler.HandleSetAvatar(store))
	mux.HandleFunc("/api/user/block", handler.HandleBlockUser())

	// Server
	mux.HandleFunc("/api/version", handler.HandleVersion(cfg))
	mux.HandleFunc("/api/server/info", handler.HandleServerInfo(cfg, store))
	mux.HandleFunc("/api/health", handler.HandleHealthCheck(store))
	mux.HandleFunc("/api/ping", handler.HandlePing)

	// Admin
	mux.HandleFunc("/api/admin/users", handler.HandleGetUsers(store))
	mux.HandleFunc("/api/admin/ban", handler.HandleBanUser(store))
	mux.HandleFunc("/api/admin/unban", handler.HandleUnbanUser(store))
	mux.HandleFunc("/api/admin/stats", handler.HandleGetStats(store, cfg))
	mux.HandleFunc("/api/admin/config", handler.HandleGetConfig(cfg))

	// Version management
	mux.HandleFunc("/api/versions", handler.HandleGetVersions(versionStore))
	mux.HandleFunc("/api/version/latest", handler.HandleGetLatestVersion(versionStore))
	mux.HandleFunc("/api/version/create", handler.HandleCreateVersion(versionStore))
	mux.HandleFunc("/api/version/delete", handler.HandleDeleteVersion(versionStore))
	mux.HandleFunc("/api/version/download", handler.HandleDownloadVersion(versionStore))
	mux.HandleFunc("/api/version/notes", handler.HandleGetReleaseNotes(versionStore))

	// Friend
	mux.HandleFunc("/api/friend/request", handler.HandleSendFriendRequest(store))
	mux.HandleFunc("/api/friend/accept", handler.HandleAcceptFriendRequest())
	mux.HandleFunc("/api/friend/reject", handler.HandleRejectFriendRequest())
	mux.HandleFunc("/api/friend/pending", handler.HandleGetFriendRequests())

	// Notification
	mux.HandleFunc("/api/notifications", handler.HandleGetNotifications())
	mux.HandleFunc("/api/notification/read", handler.HandleMarkNotificationRead())
	mux.HandleFunc("/api/notification/read-all", handler.HandleMarkAllNotificationsRead())
	mux.HandleFunc("/api/notification/delete", handler.HandleDeleteNotification())

	// Chat config
	mux.HandleFunc("/api/chat/config", handler.HandleGetChatConfig(versionStore))

	h := corsMiddleware(logMiddleware(mux))

	server := &http.Server{
		Addr:         fmt.Sprintf(":%d", cfg.Server.Port),
		Handler:      h,
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 15 * time.Second,
		IdleTimeout:  60 * time.Second,
	}

	stop := make(chan os.Signal, 1)
	signal.Notify(stop, syscall.SIGINT, syscall.SIGTERM)

	go func() {
		proto := "HTTP"
		if cfg.SSL.Enable {
			proto = "HTTPS"
		}
		log.Printf("===========================================")
		log.Printf("  Talkift Official Server")
		log.Printf("  Name:      %s", cfg.Server.Name)
		log.Printf("  Version:   %s", cfg.Version.Current)
		log.Printf("  Port:      %d (%s)", cfg.Server.Port, proto)
		log.Printf("  Chat:      %s:%d (%s)", cfg.ChatServer.Host, cfg.ChatServer.Port, cfg.ChatServer.Protocol)
		log.Printf("===========================================")
		log.Printf("  API Endpoints: 36")
		log.Printf("    Auth:         5")
		log.Printf("    User:         7")
		log.Printf("    Server:       4")
		log.Printf("    Admin:        5")
		log.Printf("    Version:      7")
		log.Printf("    Friend:       4")
		log.Printf("    Notification: 4")
		log.Printf("===========================================")

		var listenErr error
		if cfg.SSL.Enable {
			listenErr = server.ListenAndServeTLS(cfg.SSL.Cert, cfg.SSL.Key)
		} else {
			listenErr = server.ListenAndServe()
		}

		if listenErr != nil && listenErr != http.ErrServerClosed {
			log.Fatalf("Server error: %v", listenErr)
		}
	}()

	<-stop
	log.Println("Shutting down server...")

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	if err := server.Shutdown(ctx); err != nil {
		log.Fatalf("Server forced to shutdown: %v", err)
	}

	log.Println("Server stopped")
}

func corsMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type, Authorization")
		w.Header().Set("Access-Control-Max-Age", "86400")

		if r.Method == http.MethodOptions {
			w.WriteHeader(http.StatusNoContent)
			return
		}

		next.ServeHTTP(w, r)
	})
}

func logMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		wrapped := &statusResponseWriter{ResponseWriter: w, statusCode: http.StatusOK}

		next.ServeHTTP(wrapped, r)

		duration := time.Since(start)
		username := extractUsername(r)

		log.Printf("%s %s %s %d %v %s",
			r.Method, r.URL.Path, r.RemoteAddr, wrapped.statusCode, duration, username)
	})
}

type statusResponseWriter struct {
	http.ResponseWriter
	statusCode int
}

func (w *statusResponseWriter) WriteHeader(code int) {
	w.statusCode = code
	w.ResponseWriter.WriteHeader(code)
}

func extractUsername(r *http.Request) string {
	authHeader := r.Header.Get("Authorization")
	if authHeader != "" {
		token := strings.TrimPrefix(authHeader, "Bearer ")
		if token != authHeader && len(token) > 0 {
			parts := strings.SplitN(token, "|", 2)
			if len(parts) >= 1 {
				return fmt.Sprintf("user=%s", parts[0])
			}
		}
	}
	return "-"
}
