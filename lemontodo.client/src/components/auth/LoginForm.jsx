import { useState } from 'react';
import {
    Box,
    Typography,
    TextField,
    Button,
    Card,
    CardContent,
    Tabs,
    Tab,
    Alert,
    CircularProgress,
} from '@mui/material';

export function LoginForm({ onLogin, onRegister, error }) {
    const [authMode, setAuthMode] = useState(0);
    const [authForm, setAuthForm] = useState({ username: '', password: '' });
    const [isLoading, setIsLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (isLoading) return;

        setIsLoading(true);
        const { username, password } = authForm;
        
        const result = authMode === 0 
            ? await onLogin(username, password)
            : await onRegister(username, password);

        if (result.success) {
            setAuthForm({ username: '', password: '' });
        }
        setIsLoading(false);
    };

    return (
        <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', display: 'flex', alignItems: 'center', justifyContent: 'center', p: 2 }}>
            <Card sx={{ maxWidth: 480, width: '100%', boxShadow: '0 4px 24px rgba(0,0,0,0.08)' }}>
                <CardContent sx={{ p: 5 }}>
                    <Typography 
                        variant="h4" 
                        align="center" 
                        gutterBottom 
                        fontWeight={700} 
                        sx={{ 
                            mb: 4,
                            letterSpacing: '-0.03em',
                            textTransform: 'lowercase'
                        }}
                    >
                        taskflow
                    </Typography>
                    <Tabs 
                        value={authMode} 
                        onChange={(e, v) => setAuthMode(v)}
                        sx={{ mb: 4 }} 
                        centered
                    >
                        <Tab label="Sign In" sx={{ flex: 1, fontSize: '1rem' }} />
                        <Tab label="Sign Up" sx={{ flex: 1, fontSize: '1rem' }} />
                    </Tabs>
                    {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
                    <Box component="form" onSubmit={handleSubmit}>
                        <TextField 
                            fullWidth 
                            label="Username" 
                            value={authForm.username} 
                            onChange={(e) => setAuthForm({ ...authForm, username: e.target.value })} 
                            required 
                            inputProps={{ minLength: 3 }} 
                            sx={{ mb: 3 }} 
                        />
                        <TextField 
                            fullWidth 
                            type="password" 
                            label="Password" 
                            value={authForm.password} 
                            onChange={(e) => setAuthForm({ ...authForm, password: e.target.value })} 
                            required 
                            inputProps={{ minLength: 6 }} 
                            sx={{ mb: 4 }} 
                        />
                        <Button 
                            fullWidth 
                            variant="contained" 
                            size="large" 
                            type="submit" 
                            disabled={isLoading}
                            sx={{ py: 1.5 }}
                        >
                            {isLoading ? (
                                <CircularProgress size={24} sx={{ color: '#1a1a1a' }} />
                            ) : (
                                authMode === 0 ? 'Sign In' : 'Create Account'
                            )}
                        </Button>
                    </Box>
                </CardContent>
            </Card>
        </Box>
    );
}
