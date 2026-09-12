package main

import (
	"context"
	"flag"
	"fmt"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"talkift_server/internal/config"
	"talkift_server/internal/handlers"
	"talkift_server/internal/middleware"
	"talkift_server/internal/storage"
	"talkift_server/internal/utils"
	ws "talkift_server/pkg/websocket"
)

func main() {
	port := flag.Int("port", 0, "Server port (overrides config.toml)")
	flag.Parse()

	cfg, err := config.Load("config.toml")
	if err != nil {
		fmt.Fprintf(os.Stderr, "failed to load config: %v\n", err)
		os.Exit(1)
	}

	if *port > 0 {
		cfg.Server.Port = *port
	}

	utils.InitLogger(cfg.Logging.Level)
	utils.Infof("=== Talkift server starting ===")

	store, err := storage.NewLocalStorage("data")
	if err != nil {
		utils.Fatalf("failed to init storage: %v", err)
	}
	utils.Infof("storage initialized")

	if err := handlers.InitPublicGroup(store); err != nil {
		utils.Fatalf("failed to init public group: %v", err)
	}
	utils.Infof("public group initialized")

	hub := ws.NewHub()
	go hub.Run()
	utils.Infof("websocket hub started")

	h := handlers.New(store, hub, cfg)

	mux := http.NewServeMux()
	mux.HandleFunc("/ws", h.HandleWebSocket)
	mux.HandleFunc("/api/register", h.HandleRegister)
	mux.HandleFunc("/api/login", h.HandleLogin)
	mux.HandleFunc("/api/login/offline", h.HandleOfflineLogin)
	mux.HandleFunc("/api/register/thirdparty", h.HandleThirdPartyRegister)
	mux.HandleFunc("/api/conversations", middleware.AuthMiddleware(h.HandleConversations))
	mux.HandleFunc("/api/messages", middleware.AuthMiddleware(h.HandleMessages))
	mux.HandleFunc("/api/group/create", middleware.AuthMiddleware(h.HandleCreateGroup))
	mux.HandleFunc("/api/conversation/create", middleware.AuthMiddleware(h.HandleCreateConversation))
	mux.HandleFunc("/api/group/join/request", middleware.AuthMiddleware(h.HandleJoinGroupRequest))
	mux.HandleFunc("/api/group/join/response", middleware.AuthMiddleware(h.HandleJoinGroupResponse))
	mux.HandleFunc("/api/group/leave", middleware.AuthMiddleware(h.HandleLeaveGroup))
	mux.HandleFunc("/api/group/mute", middleware.AuthMiddleware(h.HandleMute))
	mux.HandleFunc("/api/dnd", middleware.AuthMiddleware(h.HandleDnd))
	mux.HandleFunc("/api/chat", middleware.AuthMiddleware(h.HandleChat))
	mux.HandleFunc("/api/chat/history", middleware.AuthMiddleware(h.HandleLoadHistory))

	addr := fmt.Sprintf(":%d", cfg.Server.Port)
	srv := &http.Server{
		Addr:         addr,
		Handler:      mux,
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 15 * time.Second,
		IdleTimeout:  60 * time.Second,
	}

	go func() {
		utils.Infof("listening on %s (ssl=%t)", addr, cfg.Server.SSLEnabled)
		if cfg.Server.SSLEnabled {
			if err := srv.ListenAndServeTLS(cfg.Server.SSLCert, cfg.Server.SSLKey); err != nil && err != http.ErrServerClosed {
				utils.Fatalf("listen error: %v", err)
			}
		} else {
			if err := srv.ListenAndServe(); err != nil && err != http.ErrServerClosed {
				utils.Fatalf("listen error: %v", err)
			}
		}
	}()

	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	utils.Infof("shutting down...")

	ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
	defer cancel()

	if err := srv.Shutdown(ctx); err != nil {
		utils.Errorf("forced shutdown: %v", err)
	}

	utils.Infof("server stopped")
}
