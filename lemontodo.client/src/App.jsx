import { useEffect, useState } from 'react';
import {
    ThemeProvider,
    createTheme,
    CssBaseline,
    Container,
    Box,
    Typography,
    TextField,
    Button,
    Card,
    CardContent,
    AppBar,
    Toolbar,
    IconButton,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Chip,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    Tabs,
    Tab,
    List,
    ListItem,
    Alert,
    CircularProgress,
    Stack,
    Paper,
    Divider,
    Menu
} from '@mui/material';
import {
    Add as AddIcon,
    CheckCircle as CheckCircleIcon,
    Delete as DeleteIcon,
    CalendarToday as CalendarIcon,
    ArrowUpward as ArrowUpwardIcon,
    ArrowDownward as ArrowDownwardIcon,
    Edit as EditIcon,
    FileDownload as FileDownloadIcon,
    FileUpload as FileUploadIcon
} from '@mui/icons-material';

import { authApi, tasksApi } from './services/api';
import { getTodayDate, getPriorityLabel, getPriorityColor, storage } from './utils/helpers';

const theme = createTheme({
    palette: {
        mode: 'light',
        primary: { main: '#1a1a1a', light: '#333333', dark: '#000000' },
        secondary: { main: '#F7C948', light: '#FFE699', dark: '#C5A03A' },
        success: { main: '#10b981' },
        error: { main: '#ef4444' },
        warning: { main: '#F7C948' },
        background: { default: '#FFFBF5', paper: '#ffffff' },
        text: { primary: '#1a1a1a', secondary: '#666666' },
    },
    typography: {
        fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
        h1: { fontWeight: 700, fontSize: '3rem' },
        h2: { fontWeight: 700, fontSize: '2.5rem' },
        h3: { fontWeight: 600, fontSize: '2rem' },
        h4: { fontWeight: 700, fontSize: '1.75rem' },
        h5: { fontWeight: 600, fontSize: '1.5rem' },
        h6: { fontWeight: 700, fontSize: '1.25rem', letterSpacing: '-0.02em' },
        button: { textTransform: 'none', fontWeight: 600 },
    },
    shape: { borderRadius: 16 },
    components: {
        MuiButton: {
            styleOverrides: {
                root: {
                    borderRadius: 32,
                    padding: '12px 32px',
                    fontSize: '1rem',
                    fontWeight: 600,
                },
                contained: {
                    boxShadow: 'none',
                    '&:hover': { boxShadow: '0 4px 12px rgba(0,0,0,0.15)' },
                },
                containedPrimary: {
                    backgroundColor: '#F7C948',
                    color: '#1a1a1a',
                    '&:hover': { backgroundColor: '#FFD966' },
                    '&:active': { backgroundColor: '#FFE699' },
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    boxShadow: '0 2px 8px rgba(0, 0, 0, 0.08)',
                    borderRadius: 16,
                },
            },
        },
        MuiAppBar: {
            styleOverrides: {
                root: { boxShadow: 'none', borderBottom: '1px solid rgba(0,0,0,0.08)' },
            },
        },
    },
});

function App() {
    const [user, setUser] = useState(null);
    const [tasks, setTasks] = useState([]);
    const [statistics, setStatistics] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isAuthLoading, setIsAuthLoading] = useState(false);
    const [authMode, setAuthMode] = useState(0);
    const [error, setError] = useState('');
    const [filter, setFilter] = useState('all');
    const [sortBy, setSortBy] = useState('createdAt');
    const [sortDirection, setSortDirection] = useState('desc');
    const [authForm, setAuthForm] = useState({ username: '', password: '' });
    const [taskForm, setTaskForm] = useState({ title: '', description: '', priority: 1, dueDate: getTodayDate() });
    const [deleteModal, setDeleteModal] = useState({ isOpen: false, taskId: null, taskTitle: '' });
    const [createModal, setCreateModal] = useState(false);
    const [editModal, setEditModal] = useState({ isOpen: false, task: null });
    const [exportMenuAnchor, setExportMenuAnchor] = useState(null);

    useEffect(() => {
        const token = storage.getToken();
        const username = storage.getUsername();
        if (token && username) {
            setUser({ username, token });
            // Validate token by attempting to load tasks
            validateAndLoadTasks(token, username);
        } else {
            setIsLoading(false);
        }
    }, []);

    const validateAndLoadTasks = async (token) => {
        try {
            const filters = { sortBy: 'createdAt', descending: true };
            const data = await tasksApi.getAll(token, filters);
            setTasks(data.tasks);
            setStatistics(data.statistics);
        } catch {
            // Token is invalid or expired, clear auth and show login
            storage.clearAuth();
            setUser(null);
            setTasks([]);
            setStatistics(null);
            setError('');
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        if (user) loadTasks(user.token);
    }, [filter, sortBy, sortDirection]);

    const loadTasks = async (token) => {
        try {
            const filters = { sortBy, descending: sortDirection === 'desc' };
            if (filter === 'completed') filters.isCompleted = 'true';
            if (filter === 'pending') filters.isCompleted = 'false';
            const data = await tasksApi.getAll(token, filters);
            setTasks(data.tasks);
            setStatistics(data.statistics);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') handleLogout();
            else setError('Failed to load tasks');
        } finally {
            setIsLoading(false);
        }
    };

    const handleAuth = async (e) => {
        e.preventDefault();
        if (isAuthLoading) return; // Prevent double-click
        setError('');
        setIsAuthLoading(true);
        try {
            const { username, password } = authForm;
            const data = authMode === 0 ? await authApi.login(username, password) : await authApi.register(username, password);
            storage.setAuth(data.token, data.username);
            setUser({ username: data.username, token: data.token });
            setAuthForm({ username: '', password: '' });
            await loadTasks(data.token);
        } catch (error) {
            setError(error.message);
            setIsAuthLoading(false); // Reset on error
        } finally {
            // Don't reset here if successful, let the redirect happen
            if (!user) {
                setIsAuthLoading(false);
            }
        }
    };

    const handleLogout = () => {
        storage.clearAuth();
        setUser(null);
        setTasks([]);
        setStatistics(null);
        setError(''); // Clear any errors
    };

    const handleCreateTask = async (e) => {
        e.preventDefault();
        setError('');
        try {
            const taskData = {
                title: taskForm.title,
                description: taskForm.description || null,
                priority: parseInt(taskForm.priority),
                dueDate: taskForm.dueDate || null
            };
            await tasksApi.create(user.token, taskData);
            setTaskForm({ title: '', description: '', priority: 1, dueDate: getTodayDate() });
            setCreateModal(false);
            await loadTasks(user.token);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') {
                setError('Session expired. Please login again.');
                handleLogout();
            } else {
                setError(error.message);
            }
        }
    };

    const handleUpdateTask = async (taskId, updates) => {
        try {
            await tasksApi.update(user.token, taskId, updates);
            await loadTasks(user.token);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') handleLogout();
            else setError('Failed to update task');
        }
    };

    const handleEditTask = async (e) => {
        e.preventDefault();
        setError('');
        try {
            const taskData = {
                title: taskForm.title,
                description: taskForm.description || null,
                priority: parseInt(taskForm.priority),
                dueDate: taskForm.dueDate || null
            };
            await tasksApi.update(user.token, editModal.task.id, taskData);
            setEditModal({ isOpen: false, task: null });
            setTaskForm({ title: '', description: '', priority: 1, dueDate: getTodayDate() });
            await loadTasks(user.token);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') {
                setError('Session expired. Please login again.');
                handleLogout();
            } else {
                setError(error.message);
            }
        }
    };

    const openEditModal = (task) => {
        setError('');
        setTaskForm({
            title: task.title,
            description: task.description || '',
            priority: task.priority,
            dueDate: task.dueDate ? task.dueDate.split('T')[0] : getTodayDate()
        });
        setEditModal({ isOpen: true, task });
    };

    const closeEditModal = () => {
        setEditModal({ isOpen: false, task: null });
        setTaskForm({ title: '', description: '', priority: 1, dueDate: getTodayDate() });
        setError('');
    };

    const handleDeleteTask = async (taskId) => {
        try {
            await tasksApi.delete(user.token, taskId);
            await loadTasks(user.token);
            setDeleteModal({ isOpen: false, taskId: null, taskTitle: '' });
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') handleLogout();
            else setError('Failed to delete task');
        }
    };

    const toggleTaskComplete = (task) => handleUpdateTask(task.id, { isCompleted: !task.isCompleted });
    const openCreateModal = () => { setError(''); setCreateModal(true); };
    const closeCreateModal = () => {
        setCreateModal(false);
        setTaskForm({ title: '', description: '', priority: 1, dueDate: getTodayDate() });
        setError('');
    };

    const handleExportTasks = async (format) => {
        try {
            await tasksApi.exportTasks(user.token, format);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') handleLogout();
            else setError(`Failed to export tasks: ${error.message}`);
        }
    };

    const handleImportTasks = async (event) => {
        const file = event.target.files?.[0];
        if (!file) return;

        try {
            const result = await tasksApi.importTasks(user.token, file);
            await loadTasks(user.token);
            setError(''); // Clear any errors
            // Show success message (you could add a success state if you want)
            console.log(result.message);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') handleLogout();
            else setError(`Failed to import tasks: ${error.message}`);
        }
        // Reset file input
        event.target.value = '';
    };

    if (isLoading) {
        return (
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <Box 
                    display="flex" 
                    flexDirection="column"
                    justifyContent="center" 
                    alignItems="center" 
                    minHeight="100vh"
                    bgcolor="background.default"
                    gap={3}
                >
                    <Typography 
                        variant="h3" 
                        sx={{ 
                            color: 'text.primary',
                            fontWeight: 700,
                            letterSpacing: '-0.03em',
                            textTransform: 'lowercase',
                            mb: 2
                        }}
                    >
                        taskflow
                    </Typography>
                    <CircularProgress size={50} thickness={4} sx={{ color: '#F7C948' }} />
                </Box>
            </ThemeProvider>
        );
    }

    if (!user) {
        return (
            <ThemeProvider theme={theme}>
                <CssBaseline />
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
                            <Tabs value={authMode} onChange={(e, v) => setAuthMode(v)} sx={{ mb: 4 }} centered>
                                <Tab label="Sign In" sx={{ flex: 1, fontSize: '1rem' }} />
                                <Tab label="Sign Up" sx={{ flex: 1, fontSize: '1rem' }} />
                            </Tabs>
                            {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
                            <Box component="form" onSubmit={handleAuth}>
                                <TextField fullWidth label="Username" value={authForm.username} onChange={(e) => setAuthForm({ ...authForm, username: e.target.value })} required inputProps={{ minLength: 3 }} sx={{ mb: 3 }} />
                                <TextField fullWidth type="password" label="Password" value={authForm.password} onChange={(e) => setAuthForm({ ...authForm, password: e.target.value })} required inputProps={{ minLength: 6 }} sx={{ mb: 4 }} />
                                <Button 
                                    fullWidth 
                                    variant="contained" 
                                    size="large" 
                                    type="submit" 
                                    disabled={isAuthLoading}
                                    sx={{ py: 1.5 }}
                                >
                                    {isAuthLoading ? (
                                        <CircularProgress size={24} sx={{ color: '#1a1a1a' }} />
                                    ) : (
                                        authMode === 0 ? 'Sign In' : 'Create Account'
                                    )}
                                </Button>
                            </Box>
                        </CardContent>
                    </Card>
                </Box>
            </ThemeProvider>
        );
    }

    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', pb: 10 }}>
                <AppBar position="static" elevation={0} sx={{ bgcolor: 'background.default', borderBottom: 'none' }}>
                    <Toolbar sx={{ py: 1.5, px: { xs: 2, sm: 4 } }}>
                        <Typography 
                            variant="h6" 
                            sx={{ 
                                flexGrow: 0,
                                mr: 6,
                                color: 'text.primary', 
                                fontWeight: 700, 
                                fontSize: '1.75rem',
                                letterSpacing: '-0.03em',
                                fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", system-ui, sans-serif',
                                textTransform: 'lowercase'
                            }}
                        >
                            taskflow
                        </Typography>
                        <Box sx={{ flexGrow: 1 }} />
                        <Stack direction="row" spacing={2} alignItems="center">
                            <Button
                                variant="outlined"
                                size="small"
                                startIcon={<FileDownloadIcon />}
                                onClick={(e) => setExportMenuAnchor(e.currentTarget)}
                                disabled={tasks.length === 0}
                                sx={{ 
                                    display: { xs: 'none', md: 'flex' },
                                    borderColor: 'divider',
                                    color: 'text.primary',
                                    '&:hover': { borderColor: 'text.primary' },
                                    '&.Mui-disabled': {
                                        borderColor: 'divider',
                                        color: 'text.disabled'
                                    }
                                }}
                            >
                                Export
                            </Button>
                            <Menu
                                anchorEl={exportMenuAnchor}
                                open={Boolean(exportMenuAnchor)}
                                onClose={() => setExportMenuAnchor(null)}
                                anchorOrigin={{
                                    vertical: 'bottom',
                                    horizontal: 'right',
                                }}
                                transformOrigin={{
                                    vertical: 'top',
                                    horizontal: 'right',
                                }}
                            >
                                <MenuItem onClick={() => handleExportTasks('json')}>
                                    Export as JSON
                                </MenuItem>
                                <MenuItem onClick={() => handleExportTasks('csv')}>
                                    Export as CSV
                                </MenuItem>
                            </Menu>
                            <Button
                                variant="outlined"
                                size="small"
                                startIcon={<FileUploadIcon />}
                                component="label"
                                sx={{ 
                                    display: { xs: 'none', md: 'flex' },
                                    borderColor: 'divider',
                                    color: 'text.primary',
                                    '&:hover': { borderColor: 'text.primary' }
                                }}
                            >
                                Import
                                <input
                                    type="file"
                                    hidden
                                    accept=".json,.csv"
                                    onChange={handleImportTasks}
                                />
                            </Button>
                            <Divider orientation="vertical" flexItem sx={{ mx: 1, display: { xs: 'none', md: 'block' } }} />
                            <Typography 
                                variant="body1" 
                                sx={{ 
                                    color: 'text.primary',
                                    fontWeight: 500,
                                    display: { xs: 'none', sm: 'block' },
                                    fontSize: '0.95rem'
                                }}
                            >
                                {user.username}
                            </Typography>
                            <Button 
                                variant="text" 
                                onClick={handleLogout} 
                                sx={{ 
                                    color: 'text.primary',
                                    fontWeight: 500,
                                    fontSize: '0.95rem',
                                    px: 2,
                                    '&:hover': { bgcolor: 'transparent', color: 'text.secondary' }
                                }}
                            >
                                Sign Out
                            </Button>
                        </Stack>
                    </Toolbar>
                </AppBar>

                <Container maxWidth="lg" sx={{ mt: 6 }}>
                    <Paper elevation={0} sx={{ p: 4, mb: 4, bgcolor: 'background.paper', border: '1px solid', borderColor: 'divider' }}>
                        <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap" sx={{ mb: 3 }}>
                            <Typography variant="h5" sx={{ flexGrow: 1, minWidth: '150px', fontWeight: 700 }}>
                                Your Tasks
                            </Typography>
                            {tasks.length > 0 && (
                                <Button 
                                    variant="contained" 
                                    startIcon={<AddIcon />} 
                                    onClick={openCreateModal}
                                    sx={{ borderRadius: 32, display: { xs: 'none', sm: 'flex' } }}
                                >
                                    New Task
                                </Button>
                            )}
                            <FormControl size="small" sx={{ minWidth: 130 }}>
                                <InputLabel>Filter</InputLabel>
                                <Select value={filter} label="Filter" onChange={(e) => setFilter(e.target.value)}>
                                    <MenuItem value="all">All Tasks</MenuItem>
                                    <MenuItem value="pending">Pending</MenuItem>
                                    <MenuItem value="completed">Completed</MenuItem>
                                </Select>
                            </FormControl>
                            <FormControl size="small" sx={{ minWidth: 150 }}>
                                <InputLabel>Sort by</InputLabel>
                                <Select value={sortBy} label="Sort by" onChange={(e) => setSortBy(e.target.value)}>
                                    <MenuItem value="createdAt">Date Created</MenuItem>
                                    <MenuItem value="priority">Priority</MenuItem>
                                    <MenuItem value="dueDate">Due Date</MenuItem>
                                    <MenuItem value="title">Title</MenuItem>
                                </Select>
                            </FormControl>
                            <IconButton size="small" onClick={() => setSortDirection(sortDirection === 'desc' ? 'asc' : 'desc')} sx={{ bgcolor: 'action.hover' }}>
                                {sortDirection === 'desc' ? <ArrowDownwardIcon fontSize="small" /> : <ArrowUpwardIcon fontSize="small" />}
                            </IconButton>
                        </Stack>

                        {/* Mobile New Task Button - only show when tasks exist */}
                        {tasks.length > 0 && (
                            <Button 
                                variant="contained" 
                                fullWidth
                                startIcon={<AddIcon />} 
                                onClick={openCreateModal}
                                sx={{ borderRadius: 32, mb: 3, display: { xs: 'flex', sm: 'none' } }}
                            >
                                New Task
                            </Button>
                        )}

                        {tasks.length === 0 ? (
                            <Box sx={{ textAlign: 'center', py: 10 }}>
                                <Typography variant="h5" color="text.primary" gutterBottom fontWeight={600}>
                                    No tasks yet
                                </Typography>
                                <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                                    Create your first task to get started
                                </Typography>
                                <Button variant="contained" size="large" startIcon={<AddIcon />} onClick={openCreateModal} sx={{ borderRadius: 32 }}>
                                    Create Your First Task
                                </Button>
                            </Box>
                        ) : (
                            <List sx={{ width: '100%' }}>
                                {tasks.map((task, index) => (
                                    <Box key={task.id}>
                                        {index > 0 && <Divider sx={{ my: 1.5 }} />}
                                        <ListItem 
                                            alignItems="center"
                                            onClick={() => openEditModal(task)}
                                            sx={{ 
                                                py: 1.5, 
                                                px: 2, 
                                                mx: -2,
                                                opacity: task.isCompleted ? 0.6 : 1, 
                                                bgcolor: 'transparent',
                                                display: 'flex',
                                                gap: 2,
                                                cursor: 'pointer',
                                                borderRadius: 2,
                                                transition: 'all 0.2s ease',
                                                '&:hover': {
                                                    bgcolor: 'action.hover',
                                                    transform: 'translateX(4px)'
                                                }
                                            }}
                                        >
                                            <Box sx={{ flex: 1, minWidth: 0 }}>
                                                <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" sx={{ mb: 0.5 }}>
                                                    <Typography variant="h6" sx={{ textDecoration: task.isCompleted ? 'line-through' : 'none', fontSize: '1rem', fontWeight: 600 }}>
                                                        {task.title}
                                                    </Typography>
                                                    <Chip label={getPriorityLabel(task.priority)} size="small" sx={{ bgcolor: getPriorityColor(task.priority), color: 'white', fontWeight: 600, height: 20, fontSize: '0.7rem' }} />
                                                    {task.completedAt && <Chip icon={<CheckCircleIcon sx={{ fontSize: 14 }} />} label={`Completed ${new Date(task.completedAt).toLocaleDateString()}`} size="small" color="success" variant="outlined" sx={{ height: 20, fontSize: '0.7rem' }} />}
                                                </Stack>

                                                {task.description && (
                                                    <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5, lineHeight: 1.4, fontSize: '0.875rem' }}>
                                                        {task.description}
                                                    </Typography>
                                                )}

                                                <Stack direction="row" spacing={1} flexWrap="wrap">
                                                    {task.dueDate && <Chip icon={<CalendarIcon sx={{ fontSize: 14 }} />} label={`Due ${new Date(task.dueDate).toLocaleDateString()}`} size="small" variant="outlined" sx={{ borderColor: 'text.secondary', color: 'text.secondary', height: 20, fontSize: '0.7rem' }} />}
                                                </Stack>
                                            </Box>

                                            <Stack direction="row" spacing={0.5} sx={{ flexShrink: 0, alignSelf: 'center' }} onClick={(e) => e.stopPropagation()}>
                                                {task.isCompleted ? (
                                                    <Button 
                                                        size="small" 
                                                        variant="outlined"
                                                        onClick={(e) => { e.stopPropagation(); toggleTaskComplete(task); }}
                                                        sx={{ borderRadius: 16, textTransform: 'none', borderColor: 'text.secondary', color: 'text.secondary', minWidth: 'auto', px: 2.4, py: 0.6, fontSize: '0.96rem' }}
                                                    >
                                                        Undo
                                                    </Button>
                                                ) : (
                                                    <Button 
                                                        size="small" 
                                                        variant="contained"
                                                        onClick={(e) => { e.stopPropagation(); toggleTaskComplete(task); }}
                                                        sx={{ 
                                                            borderRadius: 16, 
                                                            textTransform: 'none',
                                                            bgcolor: '#10b981',
                                                            color: 'white',
                                                            minWidth: 'auto',
                                                            px: 2.4,
                                                            py: 0.6,
                                                            fontSize: '0.96rem',
                                                            '&:hover': {
                                                                bgcolor: '#34d399'
                                                            }
                                                        }}
                                                    >
                                                        Complete
                                                    </Button>
                                                )}
                                                <Button 
                                                    size="small" 
                                                    variant="outlined"
                                                    onClick={(e) => { e.stopPropagation(); openEditModal(task); }}
                                                    sx={{ borderRadius: 16, textTransform: 'none', minWidth: 'auto', px: 2.4, py: 0.6, fontSize: '0.96rem' }}
                                                >
                                                    Edit
                                                </Button>
                                                <Button 
                                                    size="small" 
                                                    variant="outlined"
                                                    color="error"
                                                    onClick={(e) => { e.stopPropagation(); setDeleteModal({ isOpen: true, taskId: task.id, taskTitle: task.title }); }}
                                                    sx={{ borderRadius: 16, textTransform: 'none', minWidth: 'auto', px: 2.4, py: 0.6, fontSize: '0.96rem' }}
                                                >
                                                    Delete
                                                </Button>
                                            </Stack>
                                        </ListItem>
                                    </Box>
                                ))}
                            </List>
                        )}
                    </Paper>
                </Container>

                <Dialog open={createModal} onClose={closeCreateModal} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: 4 } }}>
                    <DialogTitle sx={{ pb: 2, pt: 3, px: 3, fontSize: '1.5rem', fontWeight: 700 }}>Create New Task</DialogTitle>
                    <form onSubmit={handleCreateTask}>
                        <DialogContent sx={{ pt: 0 }}>
                            {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
                            <TextField 
                                fullWidth 
                                label="Task Title" 
                                value={taskForm.title} 
                                onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })} 
                                required 
                                autoFocus 
                                sx={{ 
                                    mb: 3,
                                    mt: 2,
                                    '& .MuiInputLabel-root': {
                                        backgroundColor: 'background.paper',
                                        px: 1,
                                        '&.MuiInputLabel-shrink': {
                                            transform: 'translate(14px, -9px) scale(0.75)',
                                        }
                                    }
                                }} 
                            />
                            <TextField fullWidth label="Description (optional)" value={taskForm.description} onChange={(e) => setTaskForm({ ...taskForm, description: e.target.value })} multiline rows={3} sx={{ mb: 3 }} />
                            <FormControl fullWidth sx={{ mb: 3 }}>
                                <InputLabel>Priority</InputLabel>
                                <Select value={taskForm.priority} label="Priority" onChange={(e) => setTaskForm({ ...taskForm, priority: e.target.value })}>
                                    <MenuItem value={0}>Low Priority</MenuItem>
                                    <MenuItem value={1}>Medium Priority</MenuItem>
                                    <MenuItem value={2}>High Priority</MenuItem>
                                    <MenuItem value={3}>Urgent</MenuItem>
                                </Select>
                            </FormControl>
                            <TextField 
                                fullWidth 
                                label="Due Date" 
                                type="date" 
                                value={taskForm.dueDate} 
                                onChange={(e) => setTaskForm({ ...taskForm, dueDate: e.target.value })} 
                                InputLabelProps={{ shrink: true }} 
                                inputProps={{ min: getTodayDate() }}
                                onClick={(e) => {
                                    // Open date picker when clicking anywhere on the field
                                    if (e.target.type === 'date') {
                                        e.target.showPicker?.();
                                    }
                                }}
                                sx={{
                                    cursor: 'pointer',
                                    '& input[type="date"]': {
                                        cursor: 'pointer'
                                    },
                                    '& input[type="date"]::-webkit-calendar-picker-indicator': {
                                        cursor: 'pointer',
                                        opacity: 1,
                                        position: 'absolute',
                                        right: 8,
                                        width: 24,
                                        height: 24
                                    }
                                }}
                            />
                        </DialogContent>
                        <DialogActions sx={{ p: 3, pt: 2 }}>
                            <Button onClick={closeCreateModal} sx={{ color: 'text.secondary' }}>Cancel</Button>
                            <Button type="submit" variant="contained" sx={{ minWidth: 120 }}>Create Task</Button>
                        </DialogActions>
                    </form>
                </Dialog>

                {/* Edit Task Modal */}
                <Dialog open={editModal.isOpen} onClose={closeEditModal} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: 4 } }}>
                    <DialogTitle sx={{ pb: 2, pt: 3, px: 3, fontSize: '1.5rem', fontWeight: 700 }}>Edit Task</DialogTitle>
                    <form onSubmit={handleEditTask}>
                        <DialogContent sx={{ pt: 0 }}>
                            {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
                            <TextField 
                                fullWidth 
                                label="Task Title" 
                                value={taskForm.title} 
                                onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })} 
                                required 
                                autoFocus 
                                sx={{ 
                                    mb: 3,
                                    mt: 2,
                                    '& .MuiInputLabel-root': {
                                        backgroundColor: 'background.paper',
                                        px: 1,
                                        '&.MuiInputLabel-shrink': {
                                            transform: 'translate(14px, -9px) scale(0.75)',
                                        }
                                    }
                                }} 
                            />
                            <TextField fullWidth label="Description (optional)" value={taskForm.description} onChange={(e) => setTaskForm({ ...taskForm, description: e.target.value })} multiline rows={3} sx={{ mb: 3 }} />
                            <FormControl fullWidth sx={{ mb: 3 }}>
                                <InputLabel>Priority</InputLabel>
                                <Select value={taskForm.priority} label="Priority" onChange={(e) => setTaskForm({ ...taskForm, priority: e.target.value })}>
                                    <MenuItem value={0}>Low Priority</MenuItem>
                                    <MenuItem value={1}>Medium Priority</MenuItem>
                                    <MenuItem value={2}>High Priority</MenuItem>
                                    <MenuItem value={3}>Urgent</MenuItem>
                                </Select>
                            </FormControl>
                            <TextField 
                                fullWidth 
                                label="Due Date" 
                                type="date" 
                                value={taskForm.dueDate} 
                                onChange={(e) => setTaskForm({ ...taskForm, dueDate: e.target.value })} 
                                InputLabelProps={{ shrink: true }} 
                                inputProps={{ min: getTodayDate() }}
                                onClick={(e) => {
                                    if (e.target.type === 'date') {
                                        e.target.showPicker?.();
                                    }
                                }}
                                sx={{
                                    cursor: 'pointer',
                                    '& input[type="date"]': {
                                        cursor: 'pointer'
                                    },
                                    '& input[type="date"]::-webkit-calendar-picker-indicator': {
                                        cursor: 'pointer',
                                        opacity: 1,
                                        position: 'absolute',
                                        right: 8,
                                        width: 24,
                                        height: 24
                                    }
                                }}
                            />
                        </DialogContent>
                        <DialogActions sx={{ p: 3, pt: 2 }}>
                            <Button onClick={closeEditModal} sx={{ color: 'text.secondary' }}>Cancel</Button>
                            <Button type="submit" variant="contained" sx={{ minWidth: 120 }}>Update Task</Button>
                        </DialogActions>
                    </form>
                </Dialog>

                <Dialog open={deleteModal.isOpen} onClose={() => setDeleteModal({ ...deleteModal, isOpen: false })} PaperProps={{ sx: { borderRadius: 4 } }}>
                    <DialogTitle sx={{ fontSize: '1.5rem', fontWeight: 700 }}>Delete Task</DialogTitle>
                    <DialogContent>
                        <Typography sx={{ mb: 2 }}>Are you sure you want to delete this task?</Typography>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                            "{deleteModal.taskTitle}"
                        </Typography>
                    </DialogContent>
                    <DialogActions sx={{ p: 3, pt: 2 }}>
                        <Button onClick={() => setDeleteModal({ ...deleteModal, isOpen: false })} sx={{ color: 'text.secondary' }}>Cancel</Button>
                        <Button onClick={() => handleDeleteTask(deleteModal.taskId)} color="error" variant="contained">Delete</Button>
                    </DialogActions>
                </Dialog>
            </Box>
        </ThemeProvider>
    );
}

export default App;
