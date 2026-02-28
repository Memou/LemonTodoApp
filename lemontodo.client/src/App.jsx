import { useEffect, useState } from 'react';
import './App.css';
import { authApi, tasksApi } from './services/api';
import { getTodayDate, getPriorityLabel, getPriorityColor, storage } from './utils/helpers';

function App() {
    // State management
    const [user, setUser] = useState(null);
    const [tasks, setTasks] = useState([]);
    const [statistics, setStatistics] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [authMode, setAuthMode] = useState('login');
    const [error, setError] = useState('');
    const [filter, setFilter] = useState('all');
    const [sortBy, setSortBy] = useState('createdAt');

    const [authForm, setAuthForm] = useState({ username: '', password: '' });
    const [taskForm, setTaskForm] = useState({ 
        title: '', 
        description: '', 
        priority: 1, 
        dueDate: getTodayDate()
    });
    const [deleteModal, setDeleteModal] = useState({ isOpen: false, taskId: null, taskTitle: '' });
    const [createModal, setCreateModal] = useState(false);

    // Initialize app - check if user is already logged in
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

    // Reload tasks when filter or sort changes
    useEffect(() => {
        if (user) {
            loadTasks(user.token);
        }
    }, [filter, sortBy, sortDirection]);

    // Load tasks from API
    const loadTasks = async (token) => {
        try {
            const filters = {
                sortBy,
                descending: sortDirection === 'desc'
            };

            if (filter === 'completed') filters.isCompleted = 'true';
            if (filter === 'pending') filters.isCompleted = 'false';

            const data = await tasksApi.getAll(token, filters);
            setTasks(data.tasks);
            setStatistics(data.statistics);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') {
                handleLogout();
            } else {
                setError('Failed to load tasks');
            }
        } finally {
            setIsLoading(false);
        }
    };

    // Handle login or registration
    const handleAuth = async (e) => {
        e.preventDefault();
        setError('');

        try {
            const { username, password } = authForm;
            const data = authMode === 'login' 
                ? await authApi.login(username, password)
                : await authApi.register(username, password);

            storage.setAuth(data.token, data.username);
            setUser({ username: data.username, token: data.token });
            setAuthForm({ username: '', password: '' });
            await loadTasks(data.token);
        } catch (error) {
            setError(error.message);
        }
    };

    // Handle logout
    const handleLogout = () => {
        storage.clearAuth();
        setUser(null);
        setTasks([]);
        setStatistics(null);
    };

    // Create a new task
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

            setTaskForm({ 
                title: '', 
                description: '', 
                priority: 1, 
                dueDate: getTodayDate()
            });

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

    // Open create modal
    const openCreateModal = () => {
        setError('');
        setCreateModal(true);
    };

    // Close create modal
    const closeCreateModal = () => {
        setCreateModal(false);
        setTaskForm({ 
            title: '', 
            description: '', 
            priority: 1, 
            dueDate: getTodayDate()
        });
        setError('');
    };

    // Update an existing task
    const handleUpdateTask = async (taskId, updates) => {
        try {
            await tasksApi.update(user.token, taskId, updates);
            await loadTasks(user.token);
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') {
                handleLogout();
            } else {
                setError('Failed to update task');
            }
        }
    };

    // Delete a task
    const handleDeleteTask = async (taskId) => {
        try {
            await tasksApi.delete(user.token, taskId);
            await loadTasks(user.token);
            setDeleteModal({ isOpen: false, taskId: null, taskTitle: '' });
        } catch (error) {
            if (error.message === 'UNAUTHORIZED') {
                handleLogout();
            } else {
                setError('Failed to delete task');
            }
        }
    };

    // Show delete confirmation modal
    const confirmDelete = (taskId, taskTitle) => {
        setDeleteModal({ isOpen: true, taskId, taskTitle });
    };

    // Cancel delete operation
    const cancelDelete = () => {
        setDeleteModal({ isOpen: false, taskId: null, taskTitle: '' });
    };

    // Toggle task completion status
    const toggleTaskComplete = (task) => {
        handleUpdateTask(task.id, { isCompleted: !task.isCompleted });
    };

    if (isLoading) {
        return <div className="loading">Loading...</div>;
    }

    if (!user) {
        return (
            <div className="auth-container">
                <div className="auth-box">
                    <h1>✓ TaskFlow</h1>
                    <p className="subtitle">Smart Task Management</p>
                    <div className="auth-tabs">
                        <button
                            className={authMode === 'login' ? 'active' : ''}
                            onClick={() => setAuthMode('login')}
                        >
                            Login
                        </button>
                        <button
                            className={authMode === 'register' ? 'active' : ''}
                            onClick={() => setAuthMode('register')}
                        >
                            Register
                        </button>
                    </div>
                    {error && <div className="error">{error}</div>}
                    <form onSubmit={handleAuth}>
                        <input
                            type="text"
                            placeholder="Username"
                            value={authForm.username}
                            onChange={(e) => setAuthForm({ ...authForm, username: e.target.value })}
                            required
                            minLength={3}
                        />
                        <input
                            type="password"
                            placeholder="Password"
                            value={authForm.password}
                            onChange={(e) => setAuthForm({ ...authForm, password: e.target.value })}
                            required
                            minLength={6}
                        />
                        <button type="submit" className="submit-btn">
                            {authMode === 'login' ? 'Sign In' : 'Create Account'}
                        </button>
                    </form>
                </div>
            </div>
        );
    }

    return (
        <div className="app">
            <header>
                <div className="header-content">
                    <h1>✓ TaskFlow</h1>
                    <p className="header-subtitle">Stay Organized, Get Things Done</p>
                </div>
                <div className="user-info">
                    <span>👤 {user.username}</span>
                    <button onClick={handleLogout}>Logout</button>
                </div>
            </header>

            <div className="tasks-container">
                <div className="tasks-header">
                    <h2>📋 Your Tasks</h2>
                    <div className="controls">
                        <select value={filter} onChange={(e) => setFilter(e.target.value)}>
                            <option value="all">All Tasks</option>
                            <option value="pending">Pending</option>
                            <option value="completed">Completed</option>
                        </select>
                        <select value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
                            <option value="createdAt">Sort by Date</option>
                            <option value="priority">Sort by Priority</option>
                            <option value="dueDate">Sort by Due Date</option>
                            <option value="title">Sort by Title</option>
                        </select>
                        <button 
                            className="sort-direction-btn" 
                            onClick={() => setSortDirection(sortDirection === 'desc' ? 'asc' : 'desc')}
                            title={sortDirection === 'desc' ? 'Descending' : 'Ascending'}
                        >
                            {sortDirection === 'desc' ? '↓' : '↑'}
                        </button>
                    </div>
                </div>

                {tasks.length === 0 ? (
                    <div className="no-tasks-container">
                        <p className="no-tasks">No tasks yet! 🎯</p>
                        <p className="no-tasks-subtitle">Create your first task to get started.</p>
                        <button className="create-first-btn" onClick={openCreateModal}>
                            ➕ Create Your First Task
                        </button>
                    </div>
                ) : (
                    <div className="tasks-list">
                        {tasks.map(task => (
                            <div key={task.id} className={`task-card ${task.isCompleted ? 'completed' : ''}`}>
                                <div className="task-header">
                                    <h3>{task.title}</h3>
                                    <span 
                                        className="priority-badge" 
                                        style={{ backgroundColor: getPriorityColor(task.priority) }}
                                    >
                                        {getPriorityLabel(task.priority)}
                                    </span>
                                </div>
                                {task.description && (
                                    <p className="task-description">{task.description}</p>
                                )}
                                <div className="task-footer">
                                    <div className="task-dates">
                                        {task.dueDate && (
                                            <span className="due-date">
                                                📅 Due: {new Date(task.dueDate).toLocaleDateString()}
                                            </span>
                                        )}
                                        {task.completedAt && (
                                            <span className="completed-date">
                                                ✓ Completed: {new Date(task.completedAt).toLocaleDateString()}
                                            </span>
                                        )}
                                    </div>
                                    <div className="task-actions">
                                        <button 
                                            className={`complete-btn ${task.isCompleted ? 'undo-btn' : ''}`}
                                            onClick={() => toggleTaskComplete(task)}
                                        >
                                            {task.isCompleted ? '↩️ Undo' : '✓ Complete'}
                                        </button>
                                        <button 
                                            className="delete-btn"
                                            onClick={() => confirmDelete(task.id, task.title)}
                                        >
                                            🗑️ Delete
                                        </button>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            {/* Floating Action Button */}
            <button className="fab" onClick={openCreateModal} title="Create new task">
                ➕
            </button>

            {/* Create Task Modal */}
            {createModal && (
                <div className="modal-overlay" onClick={closeCreateModal}>
                    <div className="modal-content create-modal" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h3>📝 Create New Task</h3>
                            <button className="modal-close" onClick={closeCreateModal}>✕</button>
                        </div>
                        <div className="modal-body">
                            {error && <div className="error">{error}</div>}
                            <form onSubmit={handleCreateTask} className="create-task-form">
                                <input
                                    type="text"
                                    placeholder="Task title"
                                    value={taskForm.title}
                                    onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })}
                                    required
                                    autoFocus
                                />
                                <textarea
                                    placeholder="Description (optional)"
                                    value={taskForm.description}
                                    onChange={(e) => setTaskForm({ ...taskForm, description: e.target.value })}
                                    rows="4"
                                />
                                <select
                                    value={taskForm.priority}
                                    onChange={(e) => setTaskForm({ ...taskForm, priority: e.target.value })}
                                >
                                    <option value="0">Low Priority</option>
                                    <option value="1">Medium Priority</option>
                                    <option value="2">High Priority</option>
                                    <option value="3">Urgent</option>
                                </select>
                                <div className="date-input-wrapper">
                                    <label htmlFor="modalDueDate" className="date-label">📅 Due Date</label>
                                    <input
                                        id="modalDueDate"
                                        type="date"
                                        value={taskForm.dueDate}
                                        onChange={(e) => setTaskForm({ ...taskForm, dueDate: e.target.value })}
                                        min={getTodayDate()}
                                    />
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="modal-btn modal-cancel" onClick={closeCreateModal}>
                                        Cancel
                                    </button>
                                    <button type="submit" className="modal-btn modal-create">
                                        Create Task
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            {deleteModal.isOpen && (
                <div className="modal-overlay" onClick={cancelDelete}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <div className="modal-header">
                            <h3>🗑️ Delete Task</h3>
                        </div>
                        <div className="modal-body">
                            <p>Are you sure you want to delete this task?</p>
                            <p className="modal-task-title">"{deleteModal.taskTitle}"</p>
                            <p className="modal-warning">This action cannot be undone.</p>
                        </div>
                        <div className="modal-footer">
                            <button className="modal-btn modal-cancel" onClick={cancelDelete}>
                                Cancel
                            </button>
                            <button 
                                className="modal-btn modal-delete" 
                                onClick={() => handleDeleteTask(deleteModal.taskId)}
                            >
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default App;