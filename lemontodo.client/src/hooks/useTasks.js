import { useState, useEffect } from 'react';
import { tasksApi } from '../services/api';

export function useTasks(user, filter, sortBy, sortDirection) {
    const [tasks, setTasks] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');

    const loadTasks = async (token) => {
        if (!token) return;
        
        setIsLoading(true);
        try {
            const filters = { sortBy, descending: sortDirection === 'desc' };
            if (filter === 'completed') filters.isCompleted = 'true';
            if (filter === 'pending') filters.isCompleted = 'false';
            const data = await tasksApi.getAll(token, filters);
            setTasks(data.tasks);
            setError('');
        } catch (err) {
            if (err.message !== 'UNAUTHORIZED') {
                setError('Failed to load tasks');
            }
            throw err; // Re-throw to let caller handle logout
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        if (user?.token) {
            loadTasks(user.token).catch(() => {
                // Error handled in component
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [user, filter, sortBy, sortDirection]);

    const createTask = async (taskData) => {
        try {
            await tasksApi.create(user.token, taskData);
            await loadTasks(user.token);
            return { success: true };
        } catch (err) {
            return { success: false, error: err.message };
        }
    };

    const updateTask = async (taskId, updates) => {
        try {
            await tasksApi.update(user.token, taskId, updates);
            await loadTasks(user.token);
            return { success: true };
        } catch (err) {
            return { success: false, error: err.message };
        }
    };

    const deleteTask = async (taskId) => {
        try {
            await tasksApi.delete(user.token, taskId);
            await loadTasks(user.token);
            return { success: true };
        } catch (err) {
            return { success: false, error: err.message };
        }
    };

    const exportTasks = async (format) => {
        try {
            await tasksApi.exportTasks(user.token, format);
            return { success: true };
        } catch (err) {
            return { success: false, error: err.message };
        }
    };

    const importTasks = async (file) => {
        try {
            const result = await tasksApi.importTasks(user.token, file);
            await loadTasks(user.token);
            return { success: true, message: result.message };
        } catch (err) {
            return { success: false, error: err.message };
        }
    };

    return {
        tasks,
        isLoading,
        error,
        setError,
        loadTasks,
        createTask,
        updateTask,
        deleteTask,
        exportTasks,
        importTasks,
    };
}
