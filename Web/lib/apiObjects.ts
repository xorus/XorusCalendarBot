export interface Guild {
    Id: string;
    IconUrl: string;
    Name: string;
    Channels: {
        Id: string,
        Name: string,
        Category: boolean
    }[]
}

// all dates are represented as UTC strings
export interface CalendarEntity {
    Id: string,
    Name: string | null,
    CalendarEventPrefix: string,
    CalendarUrl: string,
    MaxDays: number,
    NextDateMessage: string,
    NothingPlannedMessage: string,
    GuildId: string,
    AvailableMentions: { [key: string]: string }[],
    ReminderChannel: string,
    ReminderOffsetSeconds: number,
    Sentences: string[],
    JoinedSentences?: string,
    NextSentence: number,
    NextOccurrences: CalendarEvent[],
    LastRefresh: string
}

export interface CalendarEvent {
    StartTime: string,
    NotifyTime: string,
    Summary: string | null,
    Description: string | null,
    Message: string | null,
    IsForced: boolean
}