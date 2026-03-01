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
            let errorMessage = 'Login failed';
            try {
                const error = await response.json();
                errorMessage = error.message || errorMessage;
            } catch {
                // JSON parsing failed, use status code
                errorMessage = `Login failed (${response.status})`;
            }
            throw new Error(errorMessage);
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
            let errorMessage = 'Registration failed';
            try {
                const error = await response.json();
                errorMessage = error.message || errorMessage;
            } catch {
                // JSON parsing failed, use status code
                errorMessage = `Registration failed (${response.status})`;
            }
            throw new Error(errorMessage);
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
    },

    exportTasks: async (token, format = 'json') => {
        const response = await fetch(`${API_BASE}/api/tasks/export?format=${format}`, {
            headers: getAuthHeaders(token)
        });

        if (response.status === 401) {
            throw new Error('UNAUTHORIZED');
        }

        if (!response.ok) {
            throw new Error('Failed to export tasks');
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `tasks_export_${new Date().toISOString().split('T')[0]}.${format}`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
    },

    importTasks: async (token, file) => {
        const text = await file.text();
        let tasks = [];

        if (file.name.endsWith('.json')) {
            const jsonData = JSON.parse(text);

            // Helper function to parse priority
            const parsePriority = (priority) => {
                if (typeof priority === 'number') return priority;
                const priorityStr = String(priority).toLowerCase();
                if (priorityStr.includes('low')) return 0;
                if (priorityStr.includes('medium')) return 1;
                if (priorityStr.includes('high')) return 2;
                if (priorityStr.includes('urgent')) return 3;
                return 1; // Default to medium
            };

            tasks = jsonData.map(t => ({
                id: t.Id || t.id || null,
                title: t.Title || t.title,
                description: t.Description || t.description || null,
                priority: parsePriority(t.Priority || t.priority),
                isCompleted: t.IsCompleted === true || t.isCompleted === true || false,
                dueDate: t.DueDate || t.dueDate || null
            }));
        } else if (file.name.endsWith('.csv')) {
            const lines = text.split('\n').filter(line => line.trim());
            // Skip header line
            lines.shift();

            // Helper function to parse CSV line with proper quote handling
            const parseCSVLine = (line) => {
                const values = [];
                let current = '';
                let inQuotes = false;

                for (let i = 0; i < line.length; i++) {
                    const char = line[i];

                    if (char === '"') {
                        inQuotes = !inQuotes;
                    } else if (char === ',' && !inQuotes) {
                        values.push(current.trim());
                        current = '';
                    } else {
                        current += char;
                    }
                }
                values.push(current.trim()); // Push last value

                return values;
            };

            for (let i = 0; i < lines.length; i++) {
                const values = parseCSVLine(lines[i]);

                if (values.length >= 2 && values[0] && values[1]) { // Has Id and title
                    // Parse id from first column (should be a GUID)
                    let id = null;
                    try {
                        // Check if first column is a valid GUID format
                        if (values[0].match(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i)) {
                            id = values[0];
                        }
                    } catch {
                        // Not a GUID, ignore
                    }

                    const priorityStr = values[3]?.toLowerCase() || 'medium';
                    let priority = 1;
                    if (priorityStr.includes('low')) priority = 0;
                    else if (priorityStr.includes('medium')) priority = 1;
                    else if (priorityStr.includes('high')) priority = 2;
                    else if (priorityStr.includes('urgent')) priority = 3;

                    tasks.push({
                        id: id,
                        title: values[1],
                        description: values[2] || null,
                        priority: priority,
                        isCompleted: values[4] === 'True' || values[4] === 'true',
                        dueDate: values[5] || null
                    });
                }
            }
        } else {
            throw new Error('Unsupported file format. Please use JSON or CSV.');
        }

        const response = await fetch(`${API_BASE}/api/tasks/import`, {
            method: 'POST',
            headers: getAuthHeaders(token),
            body: JSON.stringify({ tasks })
        });

        if (response.status === 401) {
            throw new Error('UNAUTHORIZED');
        }

        if (!response.ok) {
            let errorMessage = 'Failed to import tasks';
            try {
                const error = await response.json();
                // Log the full error for debugging
                console.error('Import error details:', error);
                errorMessage = error.message || error.title || errorMessage;
                // If there are validation errors, include them
                if (error.errors) {
                    const validationErrors = Object.entries(error.errors)
                        .map(([key, value]) => `${key}: ${value}`)
                        .join(', ');
                    errorMessage += ` - ${validationErrors}`;
                }
            } catch {
                errorMessage = `Failed to import tasks (${response.status})`;
            }
            throw new Error(errorMessage);
        }

        return response.json();
    }
};
