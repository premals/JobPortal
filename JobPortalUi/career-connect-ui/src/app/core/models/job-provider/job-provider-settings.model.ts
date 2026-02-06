export interface JobProviderSettings {
  interview: {
    difficultyLevels: string[];
    defaultDifficulty: string;
    questionsCount: number;
    slotDurationMinutes: number;
    slotCount: number;
  };
  ai: {
    enableShortlistSuggestions: boolean;
    enableInterviewAi: boolean;
    endpoint?: string;
    deployment?: string;
    apiVersion?: string;
    enableAvatar?: boolean;
    avatarProvider?: string;
  };
  email: {
    inviteSubject: string;
    inviteBody: string;
  };
}
