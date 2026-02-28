// API Service - Handles all API communication
const API_BASE = '';

// Helper function to get auth headers
const getAuthHeaders = (token) => ({
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
});

// Authentication API
export const authApi = {
    login: async (username, password) => {
        const response = await fetch(`${API_BASE}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Login failed');
        }
        
        return response.json();
    },

    register: async (username, password) => {
        const response = await fetch(`${API_BASE}/api/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Registration failed');
        }
        
        return response.json();
    }
};

// Tasks API
export const tasksApi = {
    getAll: async (token, filters = {}) => {
        const params = new URLSearchParams();
        
        if (filters.isCompleted !== undefined) {
            params.append('isCompleted', filters.isCompleted);
        }
        if (filters.sortBy) {
            params.append('sortBy', filters.sortBy);
        }
        if (filters.descending !== undefined) {
            params.append('descending', filters.descending);
        }

        const response = await fetch(`${API_BASE}/api/tasks?${params}`, {
            headers: getAuthHeaders(token)
        });

        if (response.status === 401) {
            throw new Error('UNAUTHORIZED');
        }
        
        if (!response.ok) {
            throw new Error('Failed to load tasks');
        }
        
        return response.json();
    },

    create: async (token, taskData) => {
        const response = await fetch(`${API_BASE}/api/tasks`, {
            method: 'POST',
            headers: getAuthHeaders(token),
            body: JSON.stringify(taskData)
        });

        if (response.status === 401) {
            throw new Error('UNAUTHORIZED');
        }
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Failed to create task');
        }
        
        return response.json();
    },

    update: async (token, taskId, updates) => {
        const response = await fetch(`${API_BASE}/api/tasks/${taskId}`, {
            method: 'PUT',
            headers: getAuthHeaders(token),
            body: JSON.stringify(updates)
        });

        if (response.status === 401) {
            throw new Error('UNAUTHORIZED');
        }
        
        if (!response.ok) {
            throw new Error('Failed to update task');
        }
        
        return response.json();
    },

    delete: async (token, taskId) => {
        const response = await fetch(`${API_BASE}/api/tasks/${taskId}`, {
            method: 'DELETE',
            headers: getAuthHeaders(token)
        });

        if (response.status === 401) {
            throw new Error('UNAUTHORIZED');
        }
        
        if (!response.ok) {
            throw new Error('Failed to delete task');
        }
        
        return response.ok;
    }
};
