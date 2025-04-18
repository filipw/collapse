from setuptools import setup, find_packages

setup(
    name="collapse",
    version="0.1.0",
    packages=find_packages(),
    install_requires=[
        "qsharp",
    ],
    entry_points={
        'console_scripts': [
            'collapse=collapse.cli:main',
        ],
    },
    python_requires='>=3.7',
    description="A CLI tool for running Q# code",
    author="filipw",
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
        "Development Status :: 4 - Beta",
        "Environment :: Console",
    ],
)