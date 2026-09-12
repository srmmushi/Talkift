package storage

import (
	"encoding/json"
	"os"
	"path/filepath"
	"sort"
	"time"

	"official-server/internal/model"
)

type VersionStorage struct {
	basePath string
	configPath string
}

func NewVersionStorage(versionPath, configPath string) *VersionStorage {
	os.MkdirAll(versionPath, 0755)
	os.MkdirAll(configPath, 0755)
	return &VersionStorage{
		basePath:   versionPath,
		configPath: configPath,
	}
}

func (v *VersionStorage) GetVersions() ([]model.VersionInfo, error) {
	entries, err := os.ReadDir(v.basePath)
	if err != nil {
		return nil, err
	}

	var versions []model.VersionInfo
	for _, entry := range entries {
		if entry.IsDir() {
			infoPath := filepath.Join(v.basePath, entry.Name(), "version.json")
			data, err := os.ReadFile(infoPath)
			if err != nil {
				continue
			}
			var info model.VersionInfo
			if err := json.Unmarshal(data, &info); err != nil {
				continue
			}
			versions = append(versions, info)
		}
	}

	sort.Slice(versions, func(i, j int) bool {
		return versions[i].Version > versions[j].Version
	})

	return versions, nil
}

func (v *VersionStorage) GetLatestVersion() (*model.VersionInfo, error) {
	versions, err := v.GetVersions()
	if err != nil {
		return nil, err
	}
	if len(versions) == 0 {
		return nil, nil
	}
	return &versions[0], nil
}

func (v *VersionStorage) GetVersion(version string) (*model.VersionInfo, error) {
	infoPath := filepath.Join(v.basePath, version, "version.json")
	data, err := os.ReadFile(infoPath)
	if err != nil {
		return nil, err
	}
	var info model.VersionInfo
	if err := json.Unmarshal(data, &info); err != nil {
		return nil, err
	}
	return &info, nil
}

func (v *VersionStorage) CreateVersion(info model.VersionInfo) error {
	versionDir := filepath.Join(v.basePath, info.Version)
	if err := os.MkdirAll(versionDir, 0755); err != nil {
		return err
	}

	data, err := json.MarshalIndent(info, "", "  ")
	if err != nil {
		return err
	}

	return os.WriteFile(filepath.Join(versionDir, "version.json"), data, 0644)
}

func (v *VersionStorage) CreateVersionWithNotes(version, notes string) error {
	info := model.VersionInfo{
		Version:      version,
		ReleaseNotes: notes,
		ReleaseDate:  time.Now().Format("2006-01-02"),
		IsLatest:     true,
	}
	return v.CreateVersion(info)
}

func (v *VersionStorage) SavePack(version string, data []byte) error {
	versionDir := filepath.Join(v.basePath, version)
	if err := os.MkdirAll(versionDir, 0755); err != nil {
		return err
	}
	return os.WriteFile(filepath.Join(versionDir, "pack.exe"), data, 0755)
}

func (v *VersionStorage) GetPackPath(version string) string {
	return filepath.Join(v.basePath, version, "pack.exe")
}

func (v *VersionStorage) SaveReleaseNotes(version, notes string) error {
	versionDir := filepath.Join(v.basePath, version)
	if err := os.MkdirAll(versionDir, 0755); err != nil {
		return err
	}
	return os.WriteFile(filepath.Join(versionDir, "version.md"), []byte(notes), 0644)
}

func (v *VersionStorage) GetReleaseNotes(version string) (string, error) {
	data, err := os.ReadFile(filepath.Join(v.basePath, version, "version.md"))
	if err != nil {
		return "", err
	}
	return string(data), nil
}

func (v *VersionStorage) GetChatServerConfig() (*model.ServerConfig, error) {
	data, err := os.ReadFile(filepath.Join(v.configPath, "chat_server.json"))
	if err != nil {
		return nil, err
	}
	var config model.ServerConfig
	if err := json.Unmarshal(data, &config); err != nil {
		return nil, err
	}
	return &config, nil
}

func (v *VersionStorage) SaveChatServerConfig(config model.ServerConfig) error {
	data, err := json.MarshalIndent(config, "", "  ")
	if err != nil {
		return err
	}
	return os.WriteFile(filepath.Join(v.configPath, "chat_server.json"), data, 0644)
}

func (v *VersionStorage) DeleteVersion(version string) error {
	versionDir := filepath.Join(v.basePath, version)
	return os.RemoveAll(versionDir)
}

func (v *VersionStorage) GetVersionCount() int {
	entries, err := os.ReadDir(v.basePath)
	if err != nil {
		return 0
	}
	count := 0
	for _, entry := range entries {
		if entry.IsDir() {
			count++
		}
	}
	return count
}
