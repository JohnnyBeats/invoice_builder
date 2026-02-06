import { ConfirmDialog } from '../../src/components/ConfirmDialog';

describe('ConfirmDialog', () => {
  it('renders nothing when closed', () => {
    cy.mount(
      <ConfirmDialog
        open={false}
        title="Delete"
        message="Are you sure?"
        onConfirm={() => {}}
        onCancel={() => {}}
      />
    );

    cy.get('body').should('not.contain.text', 'Delete');
  });

  it('renders title and message when open', () => {
    cy.mount(
      <ConfirmDialog
        open={true}
        title="Delete Customer"
        message="This action cannot be undone."
        onConfirm={() => {}}
        onCancel={() => {}}
      />
    );

    cy.contains('Delete Customer');
    cy.contains('This action cannot be undone.');
  });

  it('calls onConfirm when confirm button is clicked', () => {
    const onConfirm = cy.stub().as('confirm');

    cy.mount(
      <ConfirmDialog
        open={true}
        title="Delete"
        message="Sure?"
        onConfirm={onConfirm}
        onCancel={() => {}}
      />
    );

    cy.contains('button', 'Delete').click();
    cy.get('@confirm').should('have.been.calledOnce');
  });

  it('calls onCancel when cancel button is clicked', () => {
    const onCancel = cy.stub().as('cancel');

    cy.mount(
      <ConfirmDialog
        open={true}
        title="Delete"
        message="Sure?"
        onConfirm={() => {}}
        onCancel={onCancel}
      />
    );

    cy.contains('button', 'Cancel').click();
    cy.get('@cancel').should('have.been.calledOnce');
  });

  it('shows custom confirm label', () => {
    cy.mount(
      <ConfirmDialog
        open={true}
        title="Remove"
        message="Sure?"
        confirmLabel="Remove Item"
        onConfirm={() => {}}
        onCancel={() => {}}
      />
    );

    cy.contains('button', 'Remove Item');
  });

  it('shows loading state and disables buttons', () => {
    cy.mount(
      <ConfirmDialog
        open={true}
        title="Delete"
        message="Sure?"
        onConfirm={() => {}}
        onCancel={() => {}}
        loading={true}
      />
    );

    cy.contains('button', 'Deleting...').should('be.disabled');
    cy.contains('button', 'Cancel').should('be.disabled');
  });
});
