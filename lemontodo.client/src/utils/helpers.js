// Utility functions for the app

// Get today's date in YYYY-MM-DD format
export const getTodayDate = () => {
    return new Date().toISOString().split('T')[0];
};

// Priority labels and colors
export const getPriorityLabel = (priority) => {
    const labels = ['Low', 'Medium', 'High', 'Urgent'];
    return labels[priority] || 'Unknown';
};

export const getPriorityColor = (priority) => {
    const colors = ['#4CAF50', '#2196F3', '#FF9800', '#F44336'];
    return colors[priority] || '#999';
};

// Local storage helpers
export const storage = {
    getToken: () => localStorage.getItem('token'),
    getUsername: () => localStorage.getItem('username'),
    
    setAuth: (token, username) => {
        localStorage.setItem('token', token);
        localStorage.setItem('username', username);
    },
    
    clearAuth: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('username');
    }
};
