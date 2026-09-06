package models

import "time"

type MessageType string

const (
	MsgTypeRegister          MessageType = "register"
	MsgTypeRegisterResponse  MessageType = "register_response"
	MsgTypeLogin             MessageType = "login"
	MsgTypeLoginResponse     MessageType = "login_response"
	MsgTypeChat              MessageType = "chat"
	MsgTypeChatResponse      MessageType = "chat_response"
	MsgTypeJoinGroup         MessageType = "join_group"
	MsgTypeJoinGroupResponse MessageType = "join_group_response"
	MsgTypeJoinGroupRequest  MessageType = "join_group_request"
	MsgTypeCreateGroup       MessageType = "create_group"
	MsgTypeCreateGroupResponse MessageType = "create_group_response"
	MsgTypeLeaveGroup        MessageType = "leave_group"
	MsgTypeLeaveGroupResponse MessageType = "leave_group_response"
	MsgTypeMute              MessageType = "mute"
	MsgTypeMuteResponse      MessageType = "mute_response"
	MsgTypeDnd               MessageType = "dnd"
	MsgTypeDndResponse       MessageType = "dnd_response"
	MsgTypeLoadHistory       MessageType = "load_history"
	MsgTypeLoadHistoryResponse MessageType = "load_history_response"
	MsgTypeLogout            MessageType = "logout"
	MsgTypeHeartbeat         MessageType = "heartbeat"
	MsgTypeError             MessageType = "error"
	MsgTypeSystem            MessageType = "system"
)

type RegisterMethod string

const (
	RegisterMethodOfficial   RegisterMethod = "official"
	RegisterMethodLocal      RegisterMethod = "local"
	RegisterMethodThirdParty RegisterMethod = "third_party"
)

type MessageEnvelope struct {
	Type      MessageType `json:"type"`
	Payload   interface{} `json:"payload"`
	Timestamp time.Time   `json:"timestamp"`
}

type RegisterPayload struct {
	Username         string         `json:"username"`
	Password         string         `json:"password"`
	RegisterMethod   RegisterMethod `json:"register_method"`
	RegisterServerIP string         `json:"register_server_ip,omitempty"`
}

type RegisterResponsePayload struct {
	Success  bool   `json:"success"`
	Code     int    `json:"code"`
	UUID     string `json:"uuid,omitempty"`
	Token    string `json:"token,omitempty"`
	Username string `json:"username,omitempty"`
	Message  string `json:"message,omitempty"`
}

type LoginPayload struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

type LoginResponsePayload struct {
	Success        bool           `json:"success"`
	Code           int            `json:"code"`
	UUID           string         `json:"uuid,omitempty"`
	Token          string         `json:"token,omitempty"`
	Username       string         `json:"username,omitempty"`
	RegisterMethod RegisterMethod `json:"register_method,omitempty"`
	Message        string         `json:"message,omitempty"`
}

type ChatPayload struct {
	ConversationID string `json:"conversation_id"`
	Content        string `json:"content"`
	Type           string `json:"type"`
}

type ChatResponsePayload struct {
	Success bool   `json:"success"`
	Code    int    `json:"code"`
	Message string `json:"message,omitempty"`
}

type CreateGroupPayload struct {
	GroupName string   `json:"group_name"`
	MemberIDs []string `json:"member_ids,omitempty"`
}

type CreateGroupResponsePayload struct {
	Success      bool           `json:"success"`
	Code         int            `json:"code"`
	Conversation *Conversation  `json:"conversation,omitempty"`
	Message      string         `json:"message,omitempty"`
}

type CreateConversationPayload struct {
	ConvName     string `json:"conv_name"`
	TargetUserID string `json:"target_user_id"`
}

type JoinGroupPayload struct {
	ConversationID string `json:"conversation_id"`
}

type JoinGroupRequestPayload struct {
	ConversationID string `json:"conversation_id"`
	UserID         string `json:"user_id"`
	Username       string `json:"username"`
}

type JoinGroupResponsePayload struct {
	Success        bool   `json:"success"`
	Code           int    `json:"code"`
	ConversationID string `json:"conversation_id,omitempty"`
	UserID         string `json:"user_id,omitempty"`
	Accepted       bool   `json:"accepted"`
	Message        string `json:"message,omitempty"`
}

type LeaveGroupPayload struct {
	ConversationID string `json:"conversation_id"`
}

type MutePayload struct {
	ConversationID string `json:"conversation_id"`
	UserID         string `json:"user_id"`
}

type DndPayload struct {
	ConversationID string `json:"conversation_id"`
	Enabled        bool   `json:"enabled"`
}

type LoadHistoryPayload struct {
	ConversationID string `json:"conversation_id"`
	Limit          int    `json:"limit"`
	Before         string `json:"before,omitempty"`
}

type ErrorPayload struct {
	Code    int    `json:"code"`
	Message string `json:"message"`
}
