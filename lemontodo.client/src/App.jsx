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
    Fab,
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
    ListItemText,
    Checkbox,
    Alert,
    CircularProgress,
    Stack,
    Paper,
    Divider
} from '@mui/material';
import {
    Add as AddIcon,
    CheckCircle as CheckCircleIcon,
    RadioButtonUnchecked as RadioButtonUncheckedIcon,
    Delete as DeleteIcon,
    Logout as LogoutIcon,
    CalendarToday as CalendarIcon,
    ArrowUpward as ArrowUpwardIcon,
    ArrowDownward as ArrowDownwardIcon
} from '@mui/icons-material';

import { authApi, tasksApi } from './services/api';
import { getTodayDate, getPriorityLabel, getPriorityColor, storage } from './utils/helpers';

const theme = createTheme({
    palette: {
        mode: 'light',
        primary: { main: '#1a1a1a', light: '#333333', dark: '#000000' },
        secondary: { main: '#FFD166', light: '#FFE699', dark: '#CCAA52' },
        success: { main: '#10b981' },
        error: { main: '#ef4444' },
        warning: { main: '#FFD166' },
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
        h6: { fontWeight: 600, fontSize: '1.25rem' },
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
                    backgroundColor: '#FFD166',
                    color: '#1a1a1a',
                    '&:hover': { backgroundColor: '#CCAA52' },
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
        MuiFab: {
            styleOverrides: {
                root: {
                    backgroundColor: '#FFD166',
                    color: '#1a1a1a',
                    '&:hover': { backgroundColor: '#CCAA52' },
                },
            },
        },
    },
});

function App() {
    const [user, setUser] = useState(null);
    const [tasks, setTasks] = useState([]);
    const [statistics, setStatistics] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [authMode, setAuthMode] = useState(0);
    const [error, setError] = useState('');
    const [filter, setFilter] = useState('all');
    const [sortBy, setSortBy] = useState('createdAt');
    const [sortDirection, setSortDirection] = useState('desc');
    const [authForm, setAuthForm] = useState({ username: '', password: '' });
    const [taskForm, setTaskForm] = useState({ title: '', description: '', priority: 1, dueDate: getTodayDate() });
    const [deleteModal, setDeleteModal] = useState({ isOpen: false, taskId: null, taskTitle: '' });
    const [createModal, setCreateModal] = useState(false);

    useEffect(() => {
        const token = storage.getToken();
        const username = storage.getUsername();
        if (token && username) {
            setUser({ username, token });
            loadTasks(token);
        } else {
            setIsLoading(false);
        }
    }, []);

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
        setError('');
        try {
            const { username, password } = authForm;
            const data = authMode === 0 ? await authApi.login(username, password) : await authApi.register(username, password);
            storage.setAuth(data.token, data.username);
            setUser({ username: data.username, token: data.token });
            setAuthForm({ username: '', password: '' });
            await loadTasks(data.token);
        } catch (error) {
            setError(error.message);
        }
    };

    const handleLogout = () => {
        storage.clearAuth();
        setUser(null);
        setTasks([]);
        setStatistics(null);
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

    if (isLoading) {
        return (
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
                    <CircularProgress size={60} />
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
                            <Typography variant="h4" align="center" gutterBottom fontWeight={700} sx={{ mb: 1 }}>
                                TaskFlow
                            </Typography>
                            <Typography variant="body1" align="center" color="text.secondary" sx={{ mb: 4 }}>
                                Simple, elegant task management
                            </Typography>
                            <Tabs value={authMode} onChange={(e, v) => setAuthMode(v)} sx={{ mb: 4 }} centered>
                                <Tab label="Sign In" sx={{ flex: 1, fontSize: '1rem' }} />
                                <Tab label="Sign Up" sx={{ flex: 1, fontSize: '1rem' }} />
                            </Tabs>
                            {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
                            <Box component="form" onSubmit={handleAuth}>
                                <TextField fullWidth label="Username" value={authForm.username} onChange={(e) => setAuthForm({ ...authForm, username: e.target.value })} required inputProps={{ minLength: 3 }} sx={{ mb: 3 }} />
                                <TextField fullWidth type="password" label="Password" value={authForm.password} onChange={(e) => setAuthForm({ ...authForm, password: e.target.value })} required inputProps={{ minLength: 6 }} sx={{ mb: 4 }} />
                                <Button fullWidth variant="contained" size="large" type="submit" sx={{ py: 1.5 }}>
                                    {authMode === 0 ? 'Sign In' : 'Create Account'}
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
                <AppBar position="static" elevation={0} sx={{ bgcolor: 'background.paper' }}>
                    <Toolbar sx={{ py: 1 }}>
                        <Typography variant="h6" sx={{ flexGrow: 1, color: 'text.primary', fontWeight: 700, fontSize: '1.5rem' }}>
                            TaskFlow
                        </Typography>
                        <Typography variant="body2" sx={{ mr: 2, color: 'text.secondary' }}>
                            {user.username}
                        </Typography>
                        <Button variant="outlined" size="small" onClick={handleLogout} sx={{ borderColor: 'divider', color: 'text.primary' }}>
                            Sign Out
                        </Button>
                    </Toolbar>
                </AppBar>

                <Container maxWidth="md" sx={{ mt: 6 }}>
                    <Paper elevation={0} sx={{ p: 4, mb: 4, bgcolor: 'background.paper', border: '1px solid', borderColor: 'divider' }}>
                        <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap" sx={{ mb: 3 }}>
                            <Typography variant="h5" sx={{ flexGrow: 1, minWidth: '150px', fontWeight: 700 }}>
                                Your Tasks
                            </Typography>
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
                                        {index > 0 && <Divider sx={{ my: 2 }} />}
                                        <ListItem alignItems="flex-start" sx={{ py: 2, px: 0, opacity: task.isCompleted ? 0.6 : 1, bgcolor: 'transparent', borderRadius: 2 }}>
                                            <Checkbox checked={task.isCompleted} onChange={() => toggleTaskComplete(task)} icon={<RadioButtonUncheckedIcon />} checkedIcon={<CheckCircleIcon />} sx={{ mt: 0.5 }} />
                                            <ListItemText
                                                primary={
                                                    <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap">
                                                        <Typography variant="h6" sx={{ textDecoration: task.isCompleted ? 'line-through' : 'none', fontSize: '1.1rem', fontWeight: 600 }}>
                                                            {task.title}
                                                        </Typography>
                                                        <Chip label={getPriorityLabel(task.priority)} size="small" sx={{ bgcolor: getPriorityColor(task.priority), color: 'white', fontWeight: 600, height: 24 }} />
                                                    </Stack>
                                                }
                                                secondary={
                                                    <Box sx={{ mt: 1 }}>
                                                        {task.description && (
                                                            <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5, lineHeight: 1.6 }}>
                                                                {task.description}
                                                            </Typography>
                                                        )}
                                                        <Stack direction="row" spacing={2} flexWrap="wrap">
                                                            {task.dueDate && <Chip icon={<CalendarIcon sx={{ fontSize: 16 }} />} label={`Due ${new Date(task.dueDate).toLocaleDateString()}`} size="small" variant="outlined" sx={{ borderColor: 'text.secondary', color: 'text.secondary' }} />}
                                                            {task.completedAt && <Chip icon={<CheckCircleIcon sx={{ fontSize: 16 }} />} label={`Completed ${new Date(task.completedAt).toLocaleDateString()}`} size="small" color="success" variant="outlined" />}
                                                        </Stack>
                                                    </Box>
                                                }
                                            />
                                            <IconButton edge="end" onClick={() => setDeleteModal({ isOpen: true, taskId: task.id, taskTitle: task.title })} color="error" sx={{ mt: 0.5 }}>
                                                <DeleteIcon />
                                            </IconButton>
                                        </ListItem>
                                    </Box>
                                ))}
                            </List>
                        )}
                    </Paper>
                </Container>

                <Fab color="primary" aria-label="add" onClick={openCreateModal} sx={{ position: 'fixed', bottom: 24, right: 24 }}>
                    <AddIcon />
                </Fab>

                <Dialog open={createModal} onClose={closeCreateModal} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: 4 } }}>
                    <DialogTitle sx={{ pb: 2, fontSize: '1.5rem', fontWeight: 700 }}>Create New Task</DialogTitle>
                    <form onSubmit={handleCreateTask}>
                        <DialogContent sx={{ pt: 0 }}>
                            {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
                            <TextField fullWidth label="Task Title" value={taskForm.title} onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })} required autoFocus sx={{ mb: 3 }} />
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
                            <TextField fullWidth label="Due Date" type="date" value={taskForm.dueDate} onChange={(e) => setTaskForm({ ...taskForm, dueDate: e.target.value })} InputLabelProps={{ shrink: true }} inputProps={{ min: getTodayDate() }} />
                        </DialogContent>
                        <DialogActions sx={{ p: 3, pt: 2 }}>
                            <Button onClick={closeCreateModal} sx={{ color: 'text.secondary' }}>Cancel</Button>
                            <Button type="submit" variant="contained" sx={{ minWidth: 120 }}>Create Task</Button>
                        </DialogActions>
                    </form>
                </Dialog>

                <Dialog open={deleteModal.isOpen} onClose={() => setDeleteModal({ ...deleteModal, isOpen: false })} PaperProps={{ sx: { borderRadius: 4 } }}>
                    <DialogTitle sx={{ fontSize: '1.5rem', fontWeight: 700 }}>Delete Task</DialogTitle>
                    <DialogContent>
                        <Typography sx={{ mb: 2 }}>Are you sure you want to delete this task?</Typography>
                        <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
                            "{deleteModal.taskTitle}"
                        </Typography>
                        <Alert severity="warning">This action cannot be undone.</Alert>
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
