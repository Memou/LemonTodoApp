import { useState, useEffect } from 'react';
import { authApi } from '../services/api';
import { storage } from '../utils/helpers';

export function useAuth() {
    const [user, setUser] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const token = storage.getToken();
        const username = storage.getUsername();
        if (token && username) {
            setUser({ username: username, token: token });
        }
        setIsLoading(false);
    }, []);

    const login = async (username, password) => {
        setError('');
        try {
            const data = await authApi.login(username, password);
            storage.setAuth(data.token, data.username);
            setUser({ username: data.username, token: data.token });
            return { success: true };
        } catch (err) {
            setError(err.message);
            return { success: false, error: err.message };
        }
    };

    const register = async (username, password) => {
        setError('');
        try {
            const data = await authApi.register(username, password);
            storage.setAuth(data.token, data.username);
            setUser({ username: data.username, token: data.token });
            return { success: true };
        } catch (err) {
            setError(err.message);
            return { success: false, error: err.message };
        }
    };

    const logout = () => {
        storage.clearAuth();
        setUser(null);
        setError('');
    };

    return {
        user,
        isLoading,
        error,
        setError,
        login,
        register,
        logout,
    };
}
