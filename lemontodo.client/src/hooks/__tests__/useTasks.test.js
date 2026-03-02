import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useTasks } from '../useTasks';
import { tasksApi } from '../../services/api';

vi.mock('../../services/api');

describe('useTasks', () => {
    const mockUser = { username: 'testuser', token: 'test-token' };
    
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('initializes with empty tasks', () => {
        const { result } = renderHook(() => useTasks(null, 'all', 'createdAt', 'desc'));

        expect(result.current.tasks).toEqual([]);
        expect(result.current.isLoading).toBe(false);
        expect(result.current.error).toBe('');
    });

    it('loads tasks when user is provided', async () => {
        const mockTasks = [
            { id: '1', title: 'Task 1' },
            { id: '2', title: 'Task 2' },
        ];
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: mockTasks });

        const { result } = renderHook(() => useTasks(mockUser, 'all', 'createdAt', 'desc'));

        await waitFor(() => {
            expect(result.current.tasks).toEqual(mockTasks);
            expect(tasksApi.getAll).toHaveBeenCalledWith('test-token', {
                sortBy: 'createdAt',
                descending: true,
            });
        });
    });

    it('applies completed filter', async () => {
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        renderHook(() => useTasks(mockUser, 'completed', 'createdAt', 'desc'));

        await waitFor(() => {
            expect(tasksApi.getAll).toHaveBeenCalledWith('test-token', {
                sortBy: 'createdAt',
                descending: true,
                isCompleted: 'true',
            });
        });
    });

    it('applies pending filter', async () => {
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        renderHook(() => useTasks(mockUser, 'pending', 'createdAt', 'desc'));

        await waitFor(() => {
            expect(tasksApi.getAll).toHaveBeenCalledWith('test-token', {
                sortBy: 'createdAt',
                descending: true,
                isCompleted: 'false',
            });
        });
    });

    it('createTask successfully creates a task', async () => {
        tasksApi.create = vi.fn().mockResolvedValue({});
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        const { result } = renderHook(() => useTasks(mockUser, 'all', 'createdAt', 'desc'));

        const taskData = { title: 'New Task', priority: 1 };
        const createResult = await result.current.createTask(taskData);

        expect(createResult.success).toBe(true);
        expect(tasksApi.create).toHaveBeenCalledWith('test-token', taskData);
    });

    it('createTask handles errors', async () => {
        tasksApi.create = vi.fn().mockRejectedValue(new Error('Creation failed'));
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        const { result } = renderHook(() => useTasks(mockUser, 'all', 'createdAt', 'desc'));

        const createResult = await result.current.createTask({ title: 'New Task' });

        expect(createResult.success).toBe(false);
        expect(createResult.error).toBe('Creation failed');
    });

    it('updateTask successfully updates a task', async () => {
        tasksApi.update = vi.fn().mockResolvedValue({});
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        const { result } = renderHook(() => useTasks(mockUser, 'all', 'createdAt', 'desc'));

        const updateResult = await result.current.updateTask('task-1', { title: 'Updated' });

        expect(updateResult.success).toBe(true);
        expect(tasksApi.update).toHaveBeenCalledWith('test-token', 'task-1', { title: 'Updated' });
    });

    it('deleteTask successfully deletes a task', async () => {
        tasksApi.delete = vi.fn().mockResolvedValue({});
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        const { result } = renderHook(() => useTasks(mockUser, 'all', 'createdAt', 'desc'));

        const deleteResult = await result.current.deleteTask('task-1');

        expect(deleteResult.success).toBe(true);
        expect(tasksApi.delete).toHaveBeenCalledWith('test-token', 'task-1');
    });

    it('exportTasks calls API with correct format', async () => {
        tasksApi.exportTasks = vi.fn().mockResolvedValue({});
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        const { result } = renderHook(() => useTasks(mockUser, 'all', 'createdAt', 'desc'));

        const exportResult = await result.current.exportTasks('json');

        expect(exportResult.success).toBe(true);
        expect(tasksApi.exportTasks).toHaveBeenCalledWith('test-token', 'json');
    });

    it('importTasks successfully imports tasks', async () => {
        const mockFile = new File(['content'], 'tasks.json', { type: 'application/json' });
        tasksApi.importTasks = vi.fn().mockResolvedValue({ message: 'Imported successfully' });
        tasksApi.getAll = vi.fn().mockResolvedValue({ tasks: [] });

        const { result } = renderHook(() => useTasks(mockUser, 'all', 'createdAt', 'desc'));

        const importResult = await result.current.importTasks(mockFile);

        expect(importResult.success).toBe(true);
        expect(importResult.message).toBe('Imported successfully');
        expect(tasksApi.importTasks).toHaveBeenCalledWith('test-token', mockFile);
    });
});
