# Import necessary modules
import os

# Define the path to the todo list file
TODO_FILE = 'todo.txt'

def load_tasks():
    """Load tasks from the todo file."""
    if os.path.exists(TODO_FILE):
        with open(TODO_FILE, 'r') as file:
            return [line.strip() for line in file.readlines()]
    else:
        return []

def save_tasks(tasks):
    """Save tasks to the todo file."""
    with open(TODO_FILE, 'w') as file:
        for task in tasks:
            file.write(task + '\n')

def add_task():
    """Add a new task to the list."""
    task = input("Enter the task: ").strip()
    if task:
        tasks.append(task)
        save_tasks(tasks)
        print(f"Task added: {task}")
    else:
        print("Please enter a valid task.")

def view_tasks():
    """View all tasks in the list."""
    if not tasks:
        print("No tasks to display.")
    else:
        for idx, task in enumerate(tasks, start=1):
            print(f"{idx}. {task}")

def delete_task():
    """Delete a task from the list."""
    view_tasks()
    try:
        task_number = int(input("Enter the task number to delete: "))
        if 1 <= task_number <= len(tasks):
            deleted_task = tasks.pop(task_number - 1)
            save_tasks(tasks)
            print(f"Task deleted: {deleted_task}")
        else:
            print("Invalid task number.")
    except ValueError:
        print("Please enter a valid number.")

def main():
    """Main function to run the Todo List application."""
    global tasks
    tasks = load_tasks()
    
    while True:
        print("\n--- Todo List ---")
        print("1. Add Task")
        print("2. View Tasks")
        print("3. Delete Task")
        print("4. Exit")
        
        choice = input("Enter your choice (1-4): ")
        
        if choice == '1':
            add_task()
        elif choice == '2':
            view_tasks()
        elif choice == '3':
            delete_task()
        elif choice == '4':
            print("Exiting the application.")
            break
        else:
            print("Invalid choice. Please try again.")

if __name__ == "__main__":
    main()
