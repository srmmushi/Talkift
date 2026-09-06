package models

const (
	CodeSuccess               = 1000
	CodeUsernameExists        = 1001
	CodePasswordError         = 1002
	CodeUserNotFound          = 1003
	CodeTokenInvalid          = 1004
	CodeThirdPartyUnavailable = 1005
	CodeInvalidRequest        = 1006
	CodeInternalError         = 1007
	CodeMethodNotAllowed      = 1008
	CodeUnauthorized          = 1009
	CodeNotMember             = 1010
	CodeBanned                = 1011
	CodeIsOwner               = 1012
	CodeConversationNotFound  = 1013
	CodeAlreadyMember         = 1014
	CodeCannotLeavePublic     = 1015
	CodeTargetNotInPublic     = 1016
)

var ErrorMessages = map[int]string{
	CodeSuccess:              "success",
	CodeUsernameExists:       "username already exists",
	CodePasswordError:        "password incorrect",
	CodeUserNotFound:         "user not found",
	CodeTokenInvalid:         "invalid or expired token",
	CodeThirdPartyUnavailable: "third-party registration server unavailable",
	CodeInvalidRequest:       "invalid request",
	CodeInternalError:        "internal server error",
	CodeMethodNotAllowed:     "method not allowed",
	CodeUnauthorized:         "unauthorized",
	CodeNotMember:            "you are not a member of this conversation",
	CodeBanned:               "you are banned from this conversation",
	CodeIsOwner:              "owner cannot leave, use delete instead",
	CodeConversationNotFound: "conversation not found",
	CodeAlreadyMember:        "you are already a member",
	CodeCannotLeavePublic:    "cannot leave public group",
	CodeTargetNotInPublic:    "target user is not in the public group",
}

func ErrorMessage(code int) string {
	if msg, ok := ErrorMessages[code]; ok {
		return msg
	}
	return "unknown error"
}
