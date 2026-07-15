export interface AICategoryValidation {
    suggestedCategory: string;
    confidence: string;
    reason: string;
}

export interface AIPriorityRecommendation {
    priority: string;
    confidence: string;
    suggestedSla: string;
    reason: string;
}

export interface AISimilarComplaint {
    complaintId: number;
    similarity: string;
    complaintTitle: string;
    status: string;
    resolutionSummary: string;
}
