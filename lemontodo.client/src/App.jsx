import { useState } from 'react';
import { ThemeProvider, CssBaseline, Container, Paper, Box, Alert } from '@mui/material';
import { theme } from './theme/theme';
import { useAuth } from './hooks/useAuth';
import { useTasks } from './hooks/useTasks';
import { LoadingScreen } from './components/layout/LoadingScreen';
import { LoginForm } from './components/auth/LoginForm';
import { AppHeader } from './components/layout/AppHeader';
import { TaskFilters } from './components/tasks/TaskFilters';
import { TaskList } from './components/tasks/TaskList';
import { EmptyState } from './components/tasks/EmptyState';
import { CreateTaskModal } from './components/modals/CreateTaskModal';
import { EditTaskModal } from './components/modals/EditTaskModal';
import { DeleteTaskModal } from './components/modals/DeleteTaskModal';

function App() {
    const { user, isLoading: isAuthLoading, error: authError, login, register, logout } = useAuth();
    
    const [filter, setFilter] = useState('all');
    const [sortBy, setSortBy] = useState('createdAt');
    const [sortDirection, setSortDirection] = useState('desc');
    
    const { 
        tasks, 
        error: tasksError, 
        setError: setTasksError,
        createTask, 
        updateTask, 
        deleteTask,
        exportTasks,
        importTasks 
    } = useTasks(user, filter, sortBy, sortDirection);

    const [createModal, setCreateModal] = useState(false);
    const [editModal, setEditModal] = useState({ isOpen: false, task: null });
    const [deleteModal, setDeleteModal] = useState({ isOpen: false, taskId: null, taskTitle: '' });

    // Auth handlers
    const handleLogin = async (username, password) => {
        const result = await login(username, password);
        return result;
    };

    const handleRegister = async (username, password) => {
        const result = await register(username, password);
        return result;
    };

    const handleLogout = () => {
        logout();
        setTasksError('');
    };

    // Task handlers
    const handleCreateTask = async (taskData) => {
        const data = {
            title: taskData.title,
            description: taskData.description || null,
            priority: parseInt(taskData.priority),
            dueDate: taskData.dueDate || null
        };
        
        const result = await createTask(data);
        if (result.success) {
            setCreateModal(false);
            setTasksError('');
        } else {
            if (result.error === 'UNAUTHORIZED') {
                handleLogout();
            }
            setTasksError(result.error);
        }
    };

    const handleEditTask = async (taskData) => {
        const data = {
            title: taskData.title,
            description: taskData.description || "", // Send empty string instead of null to allow clearing
            priority: parseInt(taskData.priority),
            dueDate: taskData.dueDate || null
        };
        
        const result = await updateTask(editModal.task.id, data);
        if (result.success) {
            setEditModal({ isOpen: false, task: null });
            setTasksError('');
        } else {
            if (result.error === 'UNAUTHORIZED') {
                handleLogout();
            }
            setTasksError(result.error);
        }
    };

    const handleUpdateTask = async (taskId, updates) => {
        const result = await updateTask(taskId, updates);
        if (!result.success && result.error === 'UNAUTHORIZED') {
            handleLogout();
        }
    };

    const handleDeleteTask = async () => {
        const result = await deleteTask(deleteModal.taskId);
        if (result.success) {
            setDeleteModal({ isOpen: false, taskId: null, taskTitle: '' });
        } else {
            if (result.error === 'UNAUTHORIZED') {
                handleLogout();
            }
        }
    };

    const handleExportTasks = async (format) => {
        const result = await exportTasks(format);
        if (!result.success && result.error === 'UNAUTHORIZED') {
            handleLogout();
        }
    };

    const handleImportTasks = async (event) => {
        const file = event.target.files?.[0];
        if (!file) return;

        const result = await importTasks(file);
        if (!result.success && result.error === 'UNAUTHORIZED') {
            handleLogout();
        }
        event.target.value = '';
    };

    const openEditModal = (task) => {
        setTasksError('');
        setEditModal({ isOpen: true, task });
    };

    const closeEditModal = () => {
        setEditModal({ isOpen: false, task: null });
        setTasksError('');
    };

    const openDeleteModal = (task) => {
        setDeleteModal({ isOpen: true, taskId: task.id, taskTitle: task.title });
    };

    const closeDeleteModal = () => {
        setDeleteModal({ isOpen: false, taskId: null, taskTitle: '' });
    };

    const openCreateModal = () => {
        setTasksError('');
        setCreateModal(true);
    };

    const closeCreateModal = () => {
        setCreateModal(false);
        setTasksError('');
    };

    // Render loading screen
    if (isAuthLoading) {
        return (
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <LoadingScreen />
            </ThemeProvider>
        );
    }

    // Render login form
    if (!user) {
        return (
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <LoginForm
                    onLogin={handleLogin}
                    onRegister={handleRegister}
                    error={authError}
                />
            </ThemeProvider>
        );
    }

    // Render main app
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', pb: 10 }}>
                <AppHeader
                    username={user.username}
                    onLogout={handleLogout}
                    onExport={handleExportTasks}
                    onImport={handleImportTasks}
                    tasksCount={tasks.length}
                />

                <Container maxWidth="lg" sx={{ mt: 6 }}>
                    {tasksError && (
                        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setTasksError('')}>
                            {tasksError}
                        </Alert>
                    )}

                    <Paper elevation={0} sx={{ p: 4, mb: 4, bgcolor: 'background.paper', border: '1px solid', borderColor: 'divider' }}>
                        <TaskFilters
                            filter={filter}
                            setFilter={setFilter}
                            sortBy={sortBy}
                            setSortBy={setSortBy}
                            sortDirection={sortDirection}
                            setSortDirection={setSortDirection}
                            onCreateTask={openCreateModal}
                            hasActiveTasks={tasks.length > 0}
                        />

                        {tasks.length === 0 ? (
                            <EmptyState onCreateTask={openCreateModal} />
                        ) : (
                            <TaskList
                                tasks={tasks}
                                onEdit={openEditModal}
                                onToggleComplete={(task) => handleUpdateTask(task.id, { isCompleted: !task.isCompleted })}
                                onDelete={openDeleteModal}
                            />
                        )}
                    </Paper>
                </Container>

                {/* Modals */}
                <CreateTaskModal
                    open={createModal}
                    onClose={closeCreateModal}
                    onSubmit={handleCreateTask}
                    error={tasksError}
                />

                <EditTaskModal
                    open={editModal.isOpen}
                    task={editModal.task}
                    onClose={closeEditModal}
                    onSubmit={handleEditTask}
                    error={tasksError}
                />

                <DeleteTaskModal
                    open={deleteModal.isOpen}
                    taskTitle={deleteModal.taskTitle}
                    onClose={closeDeleteModal}
                    onConfirm={handleDeleteTask}
                />
            </Box>
        </ThemeProvider>
    );
}

export default App;
