package main

import (
	"context"
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
	cfg, err := config.Load()
	if err != nil {
		log.Fatalf("Failed to load config: %v", err)
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

	mux := http.NewServeMux()
	mux.HandleFunc("/api/register", handler.HandleRegister(store))
	mux.HandleFunc("/api/login", handler.HandleLogin(store))
	mux.HandleFunc("/api/login/email", handler.HandleLoginEmail(store))
	mux.HandleFunc("/api/verify", handler.HandleVerify(store))
	mux.HandleFunc("/api/version", handler.HandleVersion(cfg))
	mux.HandleFunc("/api/server/info", handler.HandleServerInfo(cfg))

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
		log.Printf("  UniqueId:  %s", cfg.Server.UniqueId)
		log.Printf("  Version:   %s", cfg.Version.Current)
		log.Printf("  Port:      %d (%s)", cfg.Server.Port, proto)
		log.Printf("===========================================")
		log.Printf("  Endpoints:")
		log.Printf("    POST /api/register       - Register new account")
		log.Printf("    POST /api/login          - Login with username")
		log.Printf("    POST /api/login/email    - Login with email")
		log.Printf("    POST /api/verify         - Verify token")
		log.Printf("    GET  /api/version        - Get server version")
		log.Printf("    GET  /api/server/info    - Get server info")
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
			r.Method,
			r.URL.Path,
			r.RemoteAddr,
			wrapped.statusCode,
			duration,
			username)
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
			parts := strings.SplitN(token, "|", 3)
			if len(parts) >= 1 {
				return fmt.Sprintf("user=%s", parts[0])
			}
		}
	}
	return "-"
}
