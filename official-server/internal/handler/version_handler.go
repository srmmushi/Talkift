package handler

import (
	"net/http"
	"os"

	"official-server/internal/model"
	"official-server/internal/storage"
)

func HandleGetVersions(versionStore *storage.VersionStorage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		versions, err := versionStore.GetVersions()
		if err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to get versions"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "versions": versions, "count": len(versions)})
	}
}

func HandleGetLatestVersion(versionStore *storage.VersionStorage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		version, err := versionStore.GetLatestVersion()
		if err != nil || version == nil {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "No versions found"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "version": version})
	}
}

func HandleCreateVersion(versionStore *storage.VersionStorage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodPost {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		var req struct {
			Version      string `json:"version"`
			ReleaseNotes string `json:"release_notes"`
			MinClient    string `json:"min_client"`
			UpdateUrl    string `json:"update_url"`
		}
		if err := decodeBody(r, &req); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Invalid request body"})
			return
		}

		info := model.VersionInfo{
			Version:      req.Version,
			ReleaseNotes: req.ReleaseNotes,
			MinClient:    req.MinClient,
			UpdateUrl:    req.UpdateUrl,
			IsLatest:     true,
		}

		if err := versionStore.CreateVersion(info); err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to create version"})
			return
		}

		writeJSON(w, http.StatusCreated, map[string]interface{}{"code": 0, "message": "Version created", "version": info})
	}
}

func HandleDeleteVersion(versionStore *storage.VersionStorage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodDelete {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		version := r.URL.Query().Get("version")
		if version == "" {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Version parameter is required"})
			return
		}

		if err := versionStore.DeleteVersion(version); err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]interface{}{"code": 1005, "message": "Failed to delete version"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "message": "Version deleted"})
	}
}

func HandleDownloadVersion(versionStore *storage.VersionStorage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		version := r.URL.Query().Get("version")
		if version == "" {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Version parameter is required"})
			return
		}

		packPath := versionStore.GetPackPath(version)
		if _, err := os.Stat(packPath); os.IsNotExist(err) {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "Version pack not found"})
			return
		}

		w.Header().Set("Content-Disposition", "attachment; filename=pack.exe")
		http.ServeFile(w, r, packPath)
	}
}

func HandleGetReleaseNotes(versionStore *storage.VersionStorage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		version := r.URL.Query().Get("version")
		if version == "" {
			writeJSON(w, http.StatusBadRequest, map[string]interface{}{"code": 1004, "message": "Version parameter is required"})
			return
		}

		notes, err := versionStore.GetReleaseNotes(version)
		if err != nil {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "Release notes not found"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "version": version, "notes": notes})
	}
}

func HandleGetChatConfig(versionStore *storage.VersionStorage) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if r.Method != http.MethodGet {
			writeJSON(w, http.StatusMethodNotAllowed, map[string]interface{}{"code": 1004, "message": "Method not allowed"})
			return
		}

		config, err := versionStore.GetChatServerConfig()
		if err != nil {
			writeJSON(w, http.StatusNotFound, map[string]interface{}{"code": 1003, "message": "Chat config not found"})
			return
		}

		writeJSON(w, http.StatusOK, map[string]interface{}{"code": 0, "config": config})
	}
}
