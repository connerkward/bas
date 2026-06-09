import importlib
import os

def get_solution_directories(parent_dir):
    return [name for name in os.listdir(parent_dir)
            if os.path.isdir(os.path.join(parent_dir, name)) and
            os.path.isfile(os.path.join(parent_dir, name, 'entryPoint.py'))]

def run_solution(solution_name):
    module = importlib.import_module(f'{solution_name}.entryPoint')
    module.main()

if __name__ == '__main__':
    parent_dir = os.path.dirname(os.path.abspath(__file__))
    solutions = get_solution_directories(parent_dir)
    
    if not solutions:
        print("No solutions found.")
    else:
        print("Available solutions:")
        for idx, solution in enumerate(solutions):
            print(f"{idx + 1}: {solution}")
        
        user_input = input("Enter the number of the solution you want to run or a custom solution name: ").strip()
        
        try:
            choice = int(user_input) - 1
            if 0 <= choice < len(solutions):
                run_solution(solutions[choice])
            else:
                print("Invalid number. Trying as a custom solution name.")
                run_solution(user_input)
        except ValueError:
            # If the input is not an integer, treat it as a custom solution name
            run_solution(user_input)
