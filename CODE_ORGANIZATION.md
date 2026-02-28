# Code Organization Guide 🗂️

This document explains how the code is organized to make it easier for beginners to understand.

## File Structure

```
lemontodo.client/src/
├── App.jsx                 # Main component (UI and logic)
├── App.css                 # Styling
├── services/
│   └── api.js             # All API calls (backend communication)
└── utils/
    └── helpers.js         # Helper functions (utilities)
```

## Why This Organization?

### 1. **Separation of Concerns** 🎯
- **App.jsx**: Handles UI and user interactions
- **api.js**: Handles all communication with the server
- **helpers.js**: Contains reusable utility functions

### 2. **Benefits for Beginners** 📚

#### **App.jsx** - The Main Component
- **What it does**: Manages the UI state and renders components
- **Easy to understand**: You can see what the app does without worrying about HOW it talks to the server
- **Clean functions**: Each function has a clear purpose (login, create task, delete task, etc.)

#### **services/api.js** - API Service
- **What it does**: All the fetch() calls that talk to the backend
- **Consistent structure**: Every API call follows the same pattern
- **Error handling**: Handles errors in one place
- **Easy to modify**: Need to change an API call? Just edit this file!

#### **utils/helpers.js** - Utility Functions
- **What it does**: Common functions used throughout the app
- **Reusable**: Instead of writing the same code multiple times
- **Examples**: 
  - `getTodayDate()` - Get today's date
  - `getPriorityLabel()` - Convert priority number to text
  - `storage.*` - Manage localStorage

## How The Code Works

### App.jsx Structure

```javascript
// 1. Imports (external dependencies + our services)
import { authApi, tasksApi } from './services/api';
import { getTodayDate, storage } from './utils/helpers';

// 2. State Management (all useState declarations)
const [user, setUser] = useState(null);
const [tasks, setTasks] = useState([]);
// ... more state

// 3. Effects (useEffect hooks)
useEffect(() => {
    // Initialize app on load
}, []);

// 4. Handler Functions (event handlers)
const handleAuth = async (e) => { ... };
const handleCreateTask = async (e) => { ... };
// ... more handlers

// 5. Render (JSX - what the user sees)
return (
    <div>
        {/* UI components */}
    </div>
);
```

### services/api.js Structure

```javascript
// Authentication API
export const authApi = {
    login: async (username, password) => { ... },
    register: async (username, password) => { ... }
};

// Tasks API
export const tasksApi = {
    getAll: async (token, filters) => { ... },
    create: async (token, taskData) => { ... },
    update: async (token, taskId, updates) => { ... },
    delete: async (token, taskId) => { ... }
};
```

## Using The API Service

### Before (messy)
```javascript
const handleCreateTask = async (e) => {
    e.preventDefault();
    const response = await fetch('/api/tasks', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${user.token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(taskData)
    });
    
    if (!response.ok) {
        // handle error...
    }
    // more code...
};
```

### After (clean)
```javascript
const handleCreateTask = async (e) => {
    e.preventDefault();
    try {
        await tasksApi.create(user.token, taskData);
        // success!
    } catch (error) {
        // handle error
    }
};
```

## Common Patterns

### 1. **API Call Pattern**
```javascript
const someFunction = async () => {
    try {
        const result = await apiFunction(params);
        // Do something with result
    } catch (error) {
        if (error.message === 'UNAUTHORIZED') {
            handleLogout();
        } else {
            setError(error.message);
        }
    }
};
```

### 2. **Form Submit Pattern**
```javascript
const handleSubmit = async (e) => {
    e.preventDefault();        // Prevent page reload
    setError('');             // Clear previous errors
    
    try {
        await apiCall(data);  // Call API
        resetForm();          // Reset form on success
        reloadData();         // Refresh data
    } catch (error) {
        setError(error.message);
    }
};
```

### 3. **State Update Pattern**
```javascript
// Update a single field
setFormData({ ...formData, title: newValue });

// Update with function
setTasks(prevTasks => [...prevTasks, newTask]);

// Reset form
setFormData({ title: '', description: '', priority: 1 });
```

## How to Add a New Feature

### Example: Add "Mark All Complete" Feature

#### 1. Add API function (services/api.js)
```javascript
export const tasksApi = {
    // ... existing functions
    
    markAllComplete: async (token) => {
        const response = await fetch(`${API_BASE}/api/tasks/complete-all`, {
            method: 'POST',
            headers: getAuthHeaders(token)
        });
        
        if (!response.ok) throw new Error('Failed');
        return response.json();
    }
};
```

#### 2. Add handler in App.jsx
```javascript
const handleMarkAllComplete = async () => {
    try {
        await tasksApi.markAllComplete(user.token);
        await loadTasks(user.token);
    } catch (error) {
        setError(error.message);
    }
};
```

#### 3. Add button in render
```javascript
<button onClick={handleMarkAllComplete}>
    Mark All Complete
</button>
```

## Benefits of This Structure

### ✅ **Easy to Learn**
- Each file has a single responsibility
- Functions are small and focused
- Clear naming conventions

### ✅ **Easy to Test**
- API functions can be tested separately
- Helper functions can be tested in isolation
- Component logic is separated from API logic

### ✅ **Easy to Modify**
- Need to change API endpoint? Edit api.js
- Need to add a helper? Add to helpers.js
- Need to update UI? Edit App.jsx

### ✅ **Easy to Debug**
- Errors are caught consistently
- API errors are handled in one place
- Clear flow: Component → API Service → Backend

## Tips for Beginners

1. **Start with App.jsx**: Understand what the app does
2. **Check api.js**: See how it talks to the server
3. **Look at helpers.js**: Learn common utility functions
4. **Follow the pattern**: Use the same structure for new features
5. **Error handling**: Always use try/catch with API calls
6. **Comments**: Add comments to explain complex logic

## Quick Reference

### Import the services
```javascript
import { authApi, tasksApi } from './services/api';
import { getTodayDate, storage } from './utils/helpers';
```

### Use authentication
```javascript
const data = await authApi.login(username, password);
const data = await authApi.register(username, password);
```

### Use tasks API
```javascript
const data = await tasksApi.getAll(token, filters);
await tasksApi.create(token, taskData);
await tasksApi.update(token, taskId, updates);
await tasksApi.delete(token, taskId);
```

### Use helpers
```javascript
const today = getTodayDate();
const label = getPriorityLabel(1);  // "Medium"
storage.setAuth(token, username);
storage.clearAuth();
```

## Next Steps

1. Read through App.jsx to understand the flow
2. Look at api.js to see API patterns
3. Check helpers.js for utility functions
4. Try adding a small feature using this structure!

Happy coding! 🚀
