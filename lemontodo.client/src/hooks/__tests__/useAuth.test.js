import { renderHook, waitFor, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { useAuth } from '../useAuth';
import { authApi } from '../../services/api';
import { storage } from '../../utils/helpers';

vi.mock('../../services/api');
vi.mock('../../utils/helpers');

describe('useAuth', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        storage.getToken = vi.fn();
        storage.getUsername = vi.fn();
        storage.setAuth = vi.fn();
        storage.clearAuth = vi.fn();
    });

    it('initializes with no user when storage is empty', async () => {
        storage.getToken.mockReturnValue(null);
        storage.getUsername.mockReturnValue(null);

        const { result } = renderHook(() => useAuth());

        await waitFor(() => {
            expect(result.current.isLoading).toBe(false);
        });

        expect(result.current.user).toBeNull();
    });

    it('loads user from storage on mount', async () => {
        storage.getToken.mockReturnValue('test-token');
        storage.getUsername.mockReturnValue('testuser');

        const { result } = renderHook(() => useAuth());

        await waitFor(() => {
            expect(result.current.user).toEqual({
                username: 'testuser',
                token: 'test-token',
            });
            expect(result.current.isLoading).toBe(false);
        });
    });

    it('login succeeds and sets user', async () => {
        const mockResponse = { token: 'new-token', username: 'newuser' };
        authApi.login = vi.fn().mockResolvedValue(mockResponse);

        const { result } = renderHook(() => useAuth());

        let loginResult;
        await act(async () => {
            loginResult = await result.current.login('newuser', 'password123');
        });

        expect(loginResult.success).toBe(true);
        expect(authApi.login).toHaveBeenCalledWith('newuser', 'password123');
        expect(storage.setAuth).toHaveBeenCalledWith('new-token', 'newuser');
        expect(result.current.user).toEqual({
            username: 'newuser',
            token: 'new-token',
        });
    });

    it('login fails and sets error', async () => {
        const errorMessage = 'Invalid credentials';
        authApi.login = vi.fn().mockRejectedValue(new Error(errorMessage));

        const { result } = renderHook(() => useAuth());

        let loginResult;
        await act(async () => {
            loginResult = await result.current.login('baduser', 'badpass');
        });

        expect(loginResult.success).toBe(false);
        expect(loginResult.error).toBe(errorMessage);
        expect(result.current.error).toBe(errorMessage);
        expect(result.current.user).toBeNull();
    });

    it('register succeeds and sets user', async () => {
        const mockResponse = { token: 'register-token', username: 'registereduser' };
        authApi.register = vi.fn().mockResolvedValue(mockResponse);

        const { result } = renderHook(() => useAuth());

        let registerResult;
        await act(async () => {
            registerResult = await result.current.register('registereduser', 'newpass123');
        });

        expect(registerResult.success).toBe(true);
        expect(authApi.register).toHaveBeenCalledWith('registereduser', 'newpass123');
        expect(storage.setAuth).toHaveBeenCalledWith('register-token', 'registereduser');
        expect(result.current.user).toEqual({
            username: 'registereduser',
            token: 'register-token',
        });
    });

    it('register fails and sets error', async () => {
        const errorMessage = 'Username already exists';
        authApi.register = vi.fn().mockRejectedValue(new Error(errorMessage));

        const { result } = renderHook(() => useAuth());

        let registerResult;
        await act(async () => {
            registerResult = await result.current.register('existinguser', 'password');
        });

        expect(registerResult.success).toBe(false);
        expect(registerResult.error).toBe(errorMessage);
        expect(result.current.error).toBe(errorMessage);
    });

    it('logout clears user and storage', async () => {
        storage.getToken.mockReturnValue('test-token');
        storage.getUsername.mockReturnValue('testuser');

        const { result } = renderHook(() => useAuth());

        await waitFor(() => {
            expect(result.current.user).toBeTruthy();
        });

        act(() => {
            result.current.logout();
        });

        expect(storage.clearAuth).toHaveBeenCalled();
        expect(result.current.user).toBeNull();
        expect(result.current.error).toBe('');
    });

    it('setError updates error state', () => {
        const { result } = renderHook(() => useAuth());

        act(() => {
            result.current.setError('Custom error');
        });

        expect(result.current.error).toBe('Custom error');
    });
});
