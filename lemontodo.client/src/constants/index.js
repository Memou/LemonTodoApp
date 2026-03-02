// Task priorities
export const PRIORITY = {
    LOW: 0,
    MEDIUM: 1,
    HIGH: 2,
    URGENT: 3
};

export const PRIORITY_LABELS = {
    [PRIORITY.LOW]: 'Low',
    [PRIORITY.MEDIUM]: 'Medium',
    [PRIORITY.HIGH]: 'High',
    [PRIORITY.URGENT]: 'Urgent'
};

export const PRIORITY_COLORS = {
    [PRIORITY.LOW]: '#4CAF50',
    [PRIORITY.MEDIUM]: '#2196F3',
    [PRIORITY.HIGH]: '#FF9800',
    [PRIORITY.URGENT]: '#F44336'
};

export const PRIORITY_OPTIONS = [
    { value: PRIORITY.LOW, label: 'Low Priority' },
    { value: PRIORITY.MEDIUM, label: 'Medium Priority' },
    { value: PRIORITY.HIGH, label: 'High Priority' },
    { value: PRIORITY.URGENT, label: 'Urgent' }
];

// Filter options
export const FILTER_OPTIONS = [
    { value: 'all', label: 'All Tasks' },
    { value: 'pending', label: 'Pending' },
    { value: 'completed', label: 'Completed' }
];

// Sort options
export const SORT_OPTIONS = [
    { value: 'createdAt', label: 'Date Created' },
    { value: 'priority', label: 'Priority' },
    { value: 'dueDate', label: 'Due Date' },
    { value: 'title', label: 'Title' }
];

// Error messages
export const ERROR_MESSAGES = {
    UNAUTHORIZED: 'Session expired. Please login again.',
    LOAD_TASKS_FAILED: 'Failed to load tasks',
    UPDATE_TASK_FAILED: 'Failed to update task',
    DELETE_TASK_FAILED: 'Failed to delete task',
    EXPORT_FAILED: 'Failed to export tasks',
    IMPORT_FAILED: 'Failed to import tasks'
};
