namespace EmailScanner.Domain;

public enum MailboxProvider { MicrosoftGraph = 0, Gmail = 1, Imap = 2 }
public enum MailboxConnectionStatus { Pending, Enabled, Disabled, Syncing, Failed }
public enum MailboxSyncMode { Full, Delta }
public enum EmailStatus { Queued, Processing, Processed, Failed }
public enum AttachmentStatus { Pending, Uploaded, Processing, Processed, Failed }
public enum RecipientType { To, Cc, Bcc }
public enum RuleOperator { Contains, Equals, StartsWith, EndsWith, Regex, AttachmentExists, SenderDomain }
public enum RuleActionType { TagEmail, TriggerWebhook, QueueAttachmentProcessing, MarkProcessed, MarkFailed }
public enum WebhookEventType { EmailReceived, EmailProcessed, AttachmentUploaded }
