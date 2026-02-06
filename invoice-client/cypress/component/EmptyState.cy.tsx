import { EmptyState } from '../../src/components/EmptyState';

describe('EmptyState', () => {
  it('renders title and description', () => {
    cy.mount(
      <EmptyState title="No items" description="Nothing to show here." />
    );

    cy.contains('No items');
    cy.contains('Nothing to show here.');
  });

  it('renders action button when provided', () => {
    const onClick = cy.stub().as('action');

    cy.mount(
      <EmptyState
        title="No items"
        description="Get started."
        action={<button onClick={onClick}>Add Item</button>}
      />
    );

    cy.contains('button', 'Add Item').click();
    cy.get('@action').should('have.been.calledOnce');
  });

  it('renders without action when not provided', () => {
    cy.mount(
      <EmptyState title="Empty" description="No data." />
    );

    cy.get('button').should('not.exist');
  });
});
