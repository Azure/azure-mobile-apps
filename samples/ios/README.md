# SwiftUI app

## Components

	- TextEntryControl(_ placeholder: String)
		.onSubmit { text in ... }

	- TodoItemControl(_ item: TodoItem)
		.onChange { item in ... }

	- IconButton

	- Header
		

## Views

	- TodoListView

		Header
		Refresh (ifBusy)
		List {
			ForEach item in items {
				TodoItemControl(item)
					.onChange { item in repository.saveItem(item) }
			}
			.onDelete(offsets in repository.deleteItems(offsets)
		}
		HSTack {
			TextEntryControl("Enter some text").onSubmit(...)
			IconButton(refresh).onTap(repository.synchronize())

## Repository

	@ObservableObject items = [TodoItem]()
	@ObservableObject isBusy: Bool = false

	saveItem(item: TodoItem)
	deleteItems(items: [TodoItem])
	addItem(item: TodoItem)
	synchronize()
